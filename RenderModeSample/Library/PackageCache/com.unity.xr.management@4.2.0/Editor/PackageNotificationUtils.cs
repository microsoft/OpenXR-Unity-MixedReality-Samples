using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor.XR.Management.Metadata;
using UnityEngine;
using Styles = UnityEditor.XR.Management.XRSettingsManager.Styles;

namespace UnityEditor.XR.Management
{
    /// <summary>
    /// This class holds information that should be displayed in an Editor tooltip for a given package.
    /// </summary>
    public class PackageNotificationInfo
    {
        private PackageNotificationInfo() {}

        /// <summary>
        /// Constructs a container for package notification information that displays in the XR Plug-in Management window.
        /// </summary>
        /// <param name="userInterfaceIcon">
        /// The <c>GUIContent</c> icon to display in the XR Plug-in Management window.  If the tooltip of this
        /// icon is empty, null, only whitespace, or otherwise invalid, the constructor will throw an exception.
        /// </param>
        /// <param name="additionalInfoUri">
        /// Used to surface a URI that points to additional information about the notification. For example, clicking the
        /// icon directly could send the user to the package documentation website.
        /// </param>
        /// <exception cref="ArgumentException">
        /// Thrown if either <see cref="userInterfaceIcon"/> does not contain a valid tooltip or if
        /// <see cref="additionalInfoUri"/> is not empty and isn't a valid URI string.
        /// </exception>
        public PackageNotificationInfo(GUIContent userInterfaceIcon, string tooltip, string additionalInfoUri = default)
        {
            if (string.IsNullOrWhiteSpace(tooltip) || tooltip.Length == 0)
                throw new ArgumentException("The package warning tooltip must contain a displayable message!");

            if (additionalInfoUri != default)
            {
                if (!(Uri.TryCreate(additionalInfoUri, UriKind.Absolute, out var uriResult) && (uriResult.Scheme == Uri.UriSchemeHttp || uriResult.Scheme == Uri.UriSchemeHttps)))
                    throw new ArgumentException($"The supplied information URI {additionalInfoUri} must be a well formatted URI string!");

                userInterfaceIcon.tooltip = $"{tooltip}\n\nClick the icon for additional information.";
            }
            else
                userInterfaceIcon.tooltip = tooltip;

            this.additionalInfoUri = additionalInfoUri;
            this.userInterfaceIcon = userInterfaceIcon;
        }

        /// <summary>
        /// A read-only string that contains a link to additional information about the warning.
        /// </summary>
        /// <remarks>
        /// If this is null or empty, the window will not redirect the user.
        /// </remarks>
        public readonly string additionalInfoUri;

        /// <summary>
        /// The GUI icon and tooltip that will be drawn for this <c>PackageNotificationInfo</c>.
        /// </summary>
        public readonly GUIContent userInterfaceIcon;
    }


    /// <summary>
    /// Static utility class for managing package notifications for packages.
    /// </summary>
    public static class PackageNotificationUtils
    {
        static Dictionary<string, PackageNotificationInfo> s_RegisteredPackagesWithNotifications = new Dictionary<string, PackageNotificationInfo>();

        /// <summary>
        /// Dictionary of packages that have notification to report. When a package is added to the project,
        /// that package will register itself with this container if it requires access to notification functionality.
        /// </summary>
        /// <remarks>
        /// This is a read-only dictionary and cannot be modified. To modify the dictionary, use the
        /// <see cref="RegisterPackageNotificationInformation"/> method.
        /// </remarks>
        public static IReadOnlyDictionary<string, PackageNotificationInfo> registeredPackagesWithNotifications =>
            s_RegisteredPackagesWithNotifications.ToDictionary(pair => pair.Key, pair => pair.Value);

        /// <summary>
        /// Registers a given package ID as having a notification and supplies that notification.
        /// </summary>
        /// <param name="packageId">
        /// The metadata identifier for a given package <see cref="IXRPackageMetadata.packageId"/>
        /// </param>
        /// <param name="notificationInfo">
        /// The <see cref="PackageNotificationInfo"/> for the package that corresponds to <see cref="packageId"/>.
        /// </param>
        public static void RegisterPackageNotificationInformation(string packageId, PackageNotificationInfo notificationInfo)
        {
            if (s_RegisteredPackagesWithNotifications.ContainsKey(packageId))
                s_RegisteredPackagesWithNotifications[packageId] = notificationInfo;
            else
                s_RegisteredPackagesWithNotifications.Add(packageId, notificationInfo);
        }

        const int k_RectPixelOffsetWidth = 5;

        internal static void DrawNotificationIconUI(PackageNotificationInfo notificationInfo, Rect guiRect, int pixelOffset = k_RectPixelOffsetWidth)
        {
            var position = new Vector2(guiRect.xMax - (notificationInfo.userInterfaceIcon.image.width + pixelOffset), guiRect.y);
            var size = new Vector2(notificationInfo.userInterfaceIcon.image.width, guiRect.height);
            var toolTipRect = new Rect(position, size);

            var labelStyle = EditorGUIUtility.isProSkin ? Styles.k_UrlLabelProfessional : Styles.k_UrlLabelPersonal;

            if (GUI.Button(toolTipRect, notificationInfo.userInterfaceIcon, labelStyle))
                LaunchLink(notificationInfo);
        }

        static void LaunchLink(PackageNotificationInfo info)
        {
            if (info.additionalInfoUri.Length > 0)
                Application.OpenURL(info.additionalInfoUri);
        }
    }
}
