using System;
using System.Collections.Generic;
using System.Linq;

namespace Unity.VisualScripting
{
    /// <summary>
    /// Invokes a method or a constructor via reflection.
    /// </summary>
    public sealed class InvokeMember : MemberUnit
    {
        public InvokeMember() : base() { }

        public InvokeMember(Member member) : base(member) { }

        private bool useExpandedParameters;

        /// <summary>
        /// Whether the target should be output to allow for chaining.
        /// </summary>
        [Serialize]
        [InspectableIf(nameof(supportsChaining))]
        public bool chainable { get; set; }

        [DoNotSerialize]
        public bool supportsChaining => member.requiresTarget;

        [DoNotSerialize]
        [MemberFilter(Methods = true, Constructors = true)]
        public Member invocation
        {
            get { return member; }
            set { member = value; }
        }

        [DoNotSerialize]
        [PortLabelHidden]
        public ControlInput enter { get; private set; }

        [DoNotSerialize]
        public Dictionary<int, ValueInput> inputParameters { get; private set; }

        /// <summary>
        /// The target object used when setting the value.
        /// </summary>
        [DoNotSerialize]
        [PortLabel("Target")]
        [PortLabelHidden]
        public ValueOutput targetOutput { get; private set; }

        [DoNotSerialize]
        [PortLabelHidden]
        public ValueOutput result { get; private set; }

        [DoNotSerialize]
        public Dictionary<int, ValueOutput> outputParameters { get; private set; }

        [DoNotSerialize]
        [PortLabelHidden]
        public ControlOutput exit { get; private set; }

        [DoNotSerialize]
        private int parameterCount;

        protected override void Definition()
        {
            base.Definition();

            inputParameters = new Dictionary<int, ValueInput>();
            outputParameters = new Dictionary<int, ValueOutput>();
            useExpandedParameters = true;

            enter = ControlInput(nameof(enter), Enter);
            exit = ControlOutput(nameof(exit));
            Succession(enter, exit);

            if (member.requiresTarget)
            {
                Requirement(target, enter);
            }

            if (supportsChaining && chainable)
            {
                targetOutput = ValueOutput(member.targetType, nameof(targetOutput));
                Assignment(enter, targetOutput);
            }

            if (member.isGettable)
            {
                result = ValueOutput(member.type, nameof(result), Result);

                if (member.requiresTarget)
                {
                    Requirement(target, result);
                }
            }

            var parameterInfos = member.GetParameterInfos().ToArray();

            parameterCount = parameterInfos.Length;

            for (int parameterIndex = 0; parameterIndex < parameterCount; parameterIndex++)
            {
                var parameterInfo = parameterInfos[parameterIndex];

                var parameterType = parameterInfo.UnderlyingParameterType();

                if (!parameterInfo.HasOutModifier())
                {
                    var inputParameterKey = "%" + parameterInfo.Name;

                    var inputParameter = ValueInput(parameterType, inputParameterKey);

                    inputParameters.Add(parameterIndex, inputParameter);

                    inputParameter.SetDefaultValue(parameterInfo.PseudoDefaultValue());

                    if (parameterInfo.AllowsNull())
                    {
                        inputParameter.AllowsNull();
                    }

                    Requirement(inputParameter, enter);

                    if (member.isGettable)
                    {
                        Requirement(inputParameter, result);
                    }
                }

                if (parameterInfo.ParameterType.IsByRef || parameterInfo.IsOut)
                {
                    var outputParameterKey = "&" + parameterInfo.Name;

                    var outputParameter = ValueOutput(parameterType, outputParameterKey);

                    outputParameters.Add(parameterIndex, outputParameter);

                    Assignment(enter, outputParameter);

                    useExpandedParameters = false;
                }
            }

            if (inputParameters.Count > 5)
            {
                useExpandedParameters = false;
            }
        }

        protected override bool IsMemberValid(Member member)
        {
            return member.isInvocable;
        }

        private object Invoke(object target, Flow flow)
        {
            if (useExpandedParameters)
            {
                switch (inputParameters.Count)
                {
                    case 0:

                        return member.Invoke(target);

                    case 1:

                        return member.Invoke(target,
                            flow.GetConvertedValue(inputParameters[0]));

                    case 2:

                        return member.Invoke(target,
                            flow.GetConvertedValue(inputParameters[0]),
                            flow.GetConvertedValue(inputParameters[1]));

                    case 3:

                        return member.Invoke(target,
                            flow.GetConvertedValue(inputParameters[0]),
                            flow.GetConvertedValue(inputParameters[1]),
                            flow.GetConvertedValue(inputParameters[2]));

                    case 4:

                        return member.Invoke(target,
                            flow.GetConvertedValue(inputParameters[0]),
                            flow.GetConvertedValue(inputParameters[1]),
                            flow.GetConvertedValue(inputParameters[2]),
                            flow.GetConvertedValue(inputParameters[3]));

                    case 5:

                        return member.Invoke(target,
                            flow.GetConvertedValue(inputParameters[0]),
                            flow.GetConvertedValue(inputParameters[1]),
                            flow.GetConvertedValue(inputParameters[2]),
                            flow.GetConvertedValue(inputParameters[3]),
                            flow.GetConvertedValue(inputParameters[4]));

                    default:

                        throw new NotSupportedException();
                }
            }
            else
            {
                var arguments = new object[parameterCount];

                for (int parameterIndex = 0; parameterIndex < parameterCount; parameterIndex++)
                {
                    if (inputParameters.TryGetValue(parameterIndex, out var inputParameter))
                    {
                        arguments[parameterIndex] = flow.GetConvertedValue(inputParameter);
                    }
                }

                var result = member.Invoke(target, arguments);

                for (int parameterIndex = 0; parameterIndex < parameterCount; parameterIndex++)
                {
                    if (outputParameters.TryGetValue(parameterIndex, out var outputParameter))
                    {
                        flow.SetValue(outputParameter, arguments[parameterIndex]);
                    }
                }

                return result;
            }
        }

        private object GetAndChainTarget(Flow flow)
        {
            if (member.requiresTarget)
            {
                var target = flow.GetValue(this.target, member.targetType);

                if (supportsChaining && chainable)
                {
                    flow.SetValue(targetOutput, target);
                }

                return target;
            }

            return null;
        }

        private object Result(Flow flow)
        {
            var target = GetAndChainTarget(flow);

            return Invoke(target, flow);
        }

        private ControlOutput Enter(Flow flow)
        {
            var target = GetAndChainTarget(flow);

            var result = Invoke(target, flow);

            if (this.result != null)
            {
                flow.SetValue(this.result, result);
            }

            return exit;
        }

        #region Analytics

        public override AnalyticsIdentifier GetAnalyticsIdentifier()
        {
            const int maxNumParameters = 5;
            var s = $"{member.targetType.FullName}.{member.name}";

            if (member.parameterTypes != null)
            {
                s += "(";

                for (var i = 0; i < member.parameterTypes.Length; ++i)
                {
                    if (i >= maxNumParameters)
                    {
                        s += $"->{i}";
                        break;
                    }

                    s += member.parameterTypes[i].FullName;
                    if (i < member.parameterTypes.Length - 1)
                        s += ", ";
                }

                s += ")";
            }

            var aid = new AnalyticsIdentifier
            {
                Identifier = s,
                Namespace = member.targetType.Namespace
            };
            aid.Hashcode = aid.Identifier.GetHashCode();
            return aid;
        }

        #endregion
    }
}
