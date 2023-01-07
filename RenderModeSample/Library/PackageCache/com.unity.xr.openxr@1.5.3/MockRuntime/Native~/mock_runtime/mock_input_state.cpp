#include "mock.h"

void MockInputState::Reset()
{
    switch (type)
    {
    case XR_ACTION_TYPE_BOOLEAN_INPUT:
        value.boolValue = false;
        break;

    case XR_ACTION_TYPE_FLOAT_INPUT:
        value.floatValue = false;
        break;

    case XR_ACTION_TYPE_VECTOR2F_INPUT:
        value.vectorValue = {0, 0};
        break;

    case XR_ACTION_TYPE_POSE_INPUT:
        value.locationValue.pose = {{0, 0, 0, 1}, {0, 0, 0}};
        value.locationValue.space = XR_NULL_HANDLE;
        value.locationValue.linearVelocityValid = false;
        value.locationValue.linearVelocity = {0, 0, 0};
        value.locationValue.angularVelocityValid = false;
        value.locationValue.angularVelocity = {0, 0, 0};
        break;

    default:
        break;
    }
}

void MockInputState::Set(float v)
{
    switch (type)
    {
    case XR_ACTION_TYPE_FLOAT_INPUT:
        value.floatValue = v;
        break;

    case XR_ACTION_TYPE_BOOLEAN_INPUT:
        value.boolValue = v != 0.0f;
        break;

    default:
        value.floatValue = 0.0f;
        break;
    }
}

void MockInputState::Set(XrBool32 v)
{
    switch (type)
    {
    case XR_ACTION_TYPE_BOOLEAN_INPUT:
        value.boolValue = v;
        break;

    case XR_ACTION_TYPE_FLOAT_INPUT:
        value.floatValue = v ? 1.0f : 0.0f;
        break;

    default:
        value.boolValue = false;
        break;
    }
}

void MockInputState::Set(XrVector2f v)
{
    if (type != XR_ACTION_TYPE_VECTOR2F_INPUT)
    {
        Reset();
        return;
    }

    value.vectorValue = v;
}

void MockInputState::Set(XrSpace space, XrPosef pose)
{
    if (type != XR_ACTION_TYPE_POSE_INPUT)
    {
        Reset();
        return;
    }

    value.locationValue.space = space;
    value.locationValue.pose = pose;
}

void MockInputState::SetVelocity(bool linearValid, XrVector3f linear, bool angularValid, XrVector3f angular)
{
    value.locationValue.linearVelocityValid = linearValid;
    value.locationValue.linearVelocity = linearValid ? linear : XrVector3f{0, 0, 0};
    value.locationValue.angularVelocityValid = angularValid;
    value.locationValue.angularVelocity = angularValid ? angular : XrVector3f{0, 0, 0};
}

float MockInputState::GetFloat() const
{
    switch (type)
    {
    case XR_ACTION_TYPE_BOOLEAN_INPUT:
        return (float)value.boolValue;

    case XR_ACTION_TYPE_FLOAT_INPUT:
        return value.floatValue;

    default:
        break;
    }

    return 0.0f;
}

XrBool32 MockInputState::GetBoolean() const
{
    switch (type)
    {
    case XR_ACTION_TYPE_BOOLEAN_INPUT:
        return value.boolValue;

    case XR_ACTION_TYPE_FLOAT_INPUT:
        return value.floatValue != 0.0f;

    default:
        break;
    }

    return false;
}

XrVector2f MockInputState::GetVector2() const
{
    if (type == XR_ACTION_TYPE_VECTOR2F_INPUT)
        return value.vectorValue;

    return XrVector2f();
}

XrSpace MockInputState::GetLocationSpace() const
{
    if (type == XR_ACTION_TYPE_POSE_INPUT)
        return value.locationValue.space;

    return XR_NULL_HANDLE;
}

XrPosef MockInputState::GetLocationPose() const
{
    if (type == XR_ACTION_TYPE_POSE_INPUT)
        return value.locationValue.pose;

    return XrPosef();
}

bool MockInputState::HasLinearVelocity() const
{
    return type == XR_ACTION_TYPE_POSE_INPUT && value.locationValue.linearVelocityValid;
}

XrVector3f MockInputState::GetLinearVelocity() const
{
    if (HasLinearVelocity())
        return value.locationValue.linearVelocity;

    return XrVector3f{};
}

bool MockInputState::HasAngularVelocity() const
{
    return type == XR_ACTION_TYPE_POSE_INPUT && value.locationValue.angularVelocityValid;
}

XrVector3f MockInputState::GetAngularVelocity() const
{
    if (type == XR_ACTION_TYPE_POSE_INPUT)
        return value.locationValue.angularVelocity;

    return XrVector3f{};
}

bool MockInputState::IsCompatibleType(XrActionType actionType) const
{
    switch (type)
    {
    case XR_ACTION_TYPE_BOOLEAN_INPUT:
    case XR_ACTION_TYPE_FLOAT_INPUT:
        return actionType == XR_ACTION_TYPE_FLOAT_INPUT || actionType == XR_ACTION_TYPE_BOOLEAN_INPUT;

    default:
        break;
    }

    return IsType(actionType);
}

void MockInputState::CopyValue(const MockInputState& state)
{
    switch (type)
    {
    case XR_ACTION_TYPE_BOOLEAN_INPUT:
        value.boolValue = state.GetBoolean();
        break;

    case XR_ACTION_TYPE_FLOAT_INPUT:
        value.floatValue = state.GetFloat();
        break;

    case XR_ACTION_TYPE_VECTOR2F_INPUT:
        value.vectorValue = state.GetVector2();
        break;

    case XR_ACTION_TYPE_POSE_INPUT:
        value.locationValue.space = state.GetLocationSpace();
        value.locationValue.pose = state.GetLocationPose();
        value.locationValue.angularVelocityValid = state.HasAngularVelocity();
        value.locationValue.angularVelocity = state.GetAngularVelocity();
        value.locationValue.linearVelocityValid = state.HasLinearVelocity();
        value.locationValue.linearVelocity = state.GetLinearVelocity();
        break;

    default:
        break;
    }
}
