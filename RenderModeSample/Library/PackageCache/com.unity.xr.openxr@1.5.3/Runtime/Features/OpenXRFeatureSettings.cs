using System;
using System.Collections.Generic;
using UnityEngine.XR.OpenXR.Features;
using UnityEngine.Serialization;

namespace UnityEngine.XR.OpenXR
{
    public partial class OpenXRSettings
    {
        /// <summary>
        /// All known features.
        /// </summary>
        [FormerlySerializedAs("extensions")] [HideInInspector] [SerializeField]
        internal OpenXRFeature[] features = {};

        /// <summary>
        /// Number of available features.
        /// </summary>
        public int featureCount => features.Length;

        /// <summary>
        /// Returns the first feature that matches the given type.
        /// </summary>
        /// <typeparam name="TFeature">Type of the feature to retrieve</typeparam>
        /// <returns>Feature by type</returns>
        public TFeature GetFeature<TFeature>() where TFeature : OpenXRFeature => (TFeature)GetFeature(typeof(TFeature));

        /// <summary>
        /// Returns the first feature that matches the given type.
        /// </summary>
        /// <param name="featureType">Type of the feature to return</param>
        /// <returns>Feature by type</returns>
        public OpenXRFeature GetFeature(Type featureType)
        {
            foreach(var feature in features)
                if (featureType.IsInstanceOfType(feature))
                    return feature;

            return null;
        }

        /// <summary>
        /// Returns all features of a given type.
        /// </summary>
        /// <typeparam name="TFeature">Type of the feature to retrieve</typeparam>
        /// <returns>All components of Type</returns>
        public OpenXRFeature[] GetFeatures<TFeature>() => GetFeatures(typeof(TFeature));

        /// <summary>
        /// Returns all features of Type.
        /// </summary>
        /// <param name="featureType">Type of the feature to retrieve</param>
        /// <returns>All components of Type</returns>
        public OpenXRFeature[] GetFeatures(Type featureType)
        {
            var result = new List<OpenXRFeature>();
            foreach(var feature in features)
                if (featureType.IsInstanceOfType(feature))
                    result.Add(feature);

            return result.ToArray();
        }

        /// <summary>
        /// Returns all features of a given type.
        /// </summary>
        /// <param name="featuresOut">Output list of features</param>
        /// <typeparam name="TFeature">Feature type</typeparam>
        /// <returns>Number of features returned</returns>
        public int GetFeatures<TFeature>(List<TFeature> featuresOut) where TFeature : OpenXRFeature
        {
            featuresOut.Clear();
            foreach(var feature in features)
                if (feature is TFeature xrFeature)
                    featuresOut.Add(xrFeature);

            return featuresOut.Count;
        }

        /// <summary>
        /// Returns all features of a given type.
        /// </summary>
        /// <param name="featureType">Type of the feature to retrieve</param>
        /// <param name="featuresOut">Output list of features</param>
        /// <returns>Number of features returned</returns>
        public int GetFeatures(Type featureType, List<OpenXRFeature> featuresOut)
        {
            featuresOut.Clear();
            foreach(var feature in features)
                if (featureType.IsInstanceOfType(feature))
                    featuresOut.Add(feature);

            return featuresOut.Count;
        }

        /// <summary>
        /// Return all features.
        /// </summary>
        /// <returns>All features</returns>
        public OpenXRFeature[] GetFeatures() => (OpenXRFeature[])features?.Clone() ?? new OpenXRFeature[0];

        /// <summary>
        /// Return all features.
        /// </summary>
        /// <param name="featuresOut">Output list of features</param>
        /// <returns>Number of features returned</returns>
        public int GetFeatures(List<OpenXRFeature> featuresOut)
        {
            featuresOut.Clear();
            featuresOut.AddRange(features);
            return featuresOut.Count;
        }


    }
}
