// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System;
using System.Runtime.InteropServices;
using System.Security.Cryptography.X509Certificates;
using UnityEngine;
using Unity.Collections;
using UnityEngine.XR.OpenXR;

namespace Microsoft.MixedReality.OpenXR.Remoting
{
    /// <summary>
    /// Provides information and configuration for creating a Holographic Remoting remote app.
    /// </summary>
    /// <remarks>
    /// Please note that client/server and player/remote are orthogonal concepts in remoting.
    /// Holographic Remoting remote app can act as either a
    /// Server - when listening to incoming connection from a Holographic Remoting player app (Client)
    /// (or)
    /// Client - when connecting to Holographic Remoting player app (Server) that is listening to incoming connections.
    /// For more details, please reference the <see href="https://docs.microsoft.com/windows/mixed-reality/develop/native/holographic-remoting-terminology">Holographic Remoting Terminology</see>.
    /// </remarks>
    public static class AppRemoting
    {
        /// <summary>
        /// Starts connect with given Holographic Remoting remote app (Client) configuration and initializes XR.
        /// </summary>
        /// <remarks>
        /// The remote app (Client) will try and connect to remote player (Server) listening for incoming connections, 
        /// and after a successful connection, XR experience starts. If the connection fails for any reason, try to connect by calling the coroutine again.
        /// This method must be run as a coroutine itself, as initializing XR has to happen in a coroutine.
        /// </remarks>
        /// <param name="configuration">The set of parameters to use for remoting.</param>
        [Obsolete("This method is obsolete. Use the ConnectToPlayer() method instead.", false)]
        public static System.Collections.IEnumerator Connect(RemotingConfiguration configuration)
        {
            AppRemotingSubsystem subsystem = AppRemotingSubsystem.GetCurrent();
            if(subsystem.IsAppRemotingEnabled())
            {
                yield return subsystem.ConnectLegacy(configuration);
            }
            else
            {
                Debug.LogError($"Connect is not supported, enable App Remoting feature to use this");
            }
        }

        /// <summary>
        /// Starts connect with given Holographic Remoting remote app (Client) configuration and initializes XR.
        /// </summary>
        /// <remarks>
        /// This remote app (Client) will try and connect to a remote player (Server) listening for incoming connections.
        /// After a successful connection, the XR experience will be started. Apps can use <see cref="ConnectionState"/> to 
        /// monitor the ongoing connection and use <see cref="ReadyToStart"/> to drive future connections.
        /// This method will return quickly and is safe for use in UI threads.
        /// </remarks>
        /// <param name="connectConfiguration">The set of parameters to use for remoting.</param>
        /// <remarks>
        /// During this connection, for the duration when <see cref="IsReadyToStart"/> is false, 
        /// calling <see cref="StartConnectingToPlayer"/> will be a no-op and an error message will appear in the logs.
        /// If the app wants to retry the connection, it should wait for "IsReadyToStart" to changed to true, 
        /// or monitor the "ReadyToStart" event.
        /// </remarks> 
        public static void StartConnectingToPlayer(RemotingConnectConfiguration connectConfiguration)
        {
            AppRemotingSubsystem subsystem = AppRemotingSubsystem.GetCurrent();
            if(subsystem.IsAppRemotingEnabled())
            {
                subsystem.StartConnecting(connectConfiguration);
            }
            else
            {
                Debug.LogError($"ConnectToPlayer is not supported, enable App Remoting feature to use this.");
            }
        }

        /// <summary>
        /// Starts listen with given Holographic Remoting remote app (Server) configuration and initializes XR.
        /// </summary>
        /// <remarks>
        /// The remote app (Server) will be waiting for remote player (Client) to connect, and after a successful connection, XR experience starts. 
        /// If the connection fails for any reason, it will retry listening for incoming connection until some other method is called.
        /// This method must be run as a coroutine itself, as initializing XR has to happen in a coroutine.
        /// </remarks>
        /// <param name="listenConfiguration">The set of parameters to use for remoting.</param>
        /// <param name="onRemotingListenCompleted"> Action callback to signal listen complete. A new Connect or Listen coroutine can safely be started after this callback.</param>
        /// <remarks>
        /// During a Listen coroutine, if the remote app calls <see cref="Listen"/> or <see cref="Connect"/> function, 
        /// the second call will fail, because there can only be a single outstanding remoting connection. The Listen coroutine will wait indefinitely for new connections 
        /// until the remote app calls <see cref="Disconnect"/> function to stop listening.  During this coroutine, there could be multiple remoting connections 
        /// and the <see cref="ConnectionState"/> may change multiple times. If the remote app wants to know the completion of above listening session, 
        /// it can use the `onRemotingListenCompleted` callback here. After the callback, the Listen coroutine will complete, and the application can safely call the `Connect` or `Listen` functions again. 
        /// </remarks>
        [Obsolete("This method is obsolete. Use the StartListeningForPlayer() instead.", false)]
        public static System.Collections.IEnumerator Listen(RemotingListenConfiguration listenConfiguration, Action onRemotingListenCompleted = null)
        {
            AppRemotingSubsystem subsystem = AppRemotingSubsystem.GetCurrent();
            if(subsystem.IsAppRemotingEnabled())
            {
                yield return subsystem.ListenLegacy(listenConfiguration, ListenMode.LegacyListen, onRemotingListenCompleted);
            }
            else
            {
                Debug.LogError($"Listen is not supported, enable App Remoting feature to use this.");
                if (onRemotingListenCompleted != null)
                {
                    onRemotingListenCompleted.Invoke();
                }
            }
        }

        /// <summary>
        /// Starts listen with given Holographic Remoting remote app (Server) configuration and initializes XR.
        /// </summary>
        /// <remarks>
        /// The remote app (Server) will be waiting for remote player (Client) to connect, and after a successful connection, XR experience starts. 
        /// This method will return quickly and is safe for use in UI threads. Apps can use <see cref="ConnectionState"/> to monitor the ongoing connection.
        /// If the connection fails for any reason, it will retry listening for incoming connection until <see cref="StopListening"/> or <see cref="Disconnect"/> is called.
        /// </remarks>
        /// <param name="listenConfiguration">The set of parameters to use for remoting.</param>
        /// <remarks>
        /// During this connection, for the duration when <see cref="IsReadyToStart"/> is false, 
        /// calling <see cref="StartListeningForPlayer"/> will be a no-op and an error message will appear in the logs.
        /// If the app wants to retry the connection, it should wait for "IsReadyToStart" to changed to true, 
        /// or monitor the "ReadyToStart" event.
        /// </remarks>
        public static void StartListeningForPlayer(RemotingListenConfiguration listenConfiguration)
        {
            AppRemotingSubsystem subsystem = AppRemotingSubsystem.GetCurrent();
            if(subsystem.IsAppRemotingEnabled())
            {
                subsystem.StartListening(listenConfiguration, ListenMode.Listen, null);
            }
            else
            {
                Debug.LogError($"StartListeningForPlayer is not supported, enable App Remoting feature to use this.");
            }
        }

        /// <summary>
        /// Disconnects the remote app (Client/Server) from the remote player (Client/Server) and stops the active XR session.
        /// </summary>
        /// <remarks>
        /// Disconnects network connection between remote app and remote player, stops <see cref="ConnectToPlayer"/>, <see cref="Connect"/> and <see cref="Listen"/> coroutines. 
        /// It does not stop <see cref="StartListeningForPlayer"/> coroutine, use <see cref="StopListening"/> instead. `Disconnect` is not equivalent to the completion of `ConnectToPlayer` coroutine,
        /// please use a wrapper coroutine, as explained in <see cref="ConnectToPlayer"/> to identify completion.
        /// </remarks>
        public static void Disconnect()
        {
            AppRemotingSubsystem subsystem = AppRemotingSubsystem.GetCurrent();
            if(subsystem.IsAppRemotingEnabled())
            {
                subsystem.Disconnect();
            }
            else
            {
                Debug.LogError($"Disconnect is not supported, enable App Remoting feature to use this.");
            }
        }

        /// <summary>
        /// Stops listening on the remote app (Server) for incoming connections from the remote player (Client) and stops the active XR session.
        /// </summary>
        /// <remarks>
        /// Disconnects any outstanding Listen session and exits <see cref="StartListeningForPlayer"/> coroutine, throws an error if used with <see cref="Listen"/> coroutine.
        /// `StopListening` is not equivalent to the completion of `StartListeningForPlayer` coroutine, please use a wrapper coroutine as explained in <see cref="StartListeningForPlayer"/> 
        /// to identify completion.
        /// </remarks>
        public static void StopListening()
        {
            AppRemotingSubsystem subsystem = AppRemotingSubsystem.GetCurrent();
            if(subsystem.IsAppRemotingEnabled())
            {
                subsystem.StopListening();
            }
            else
            {
                Debug.LogError($"StopListening is not supported, enable App Remoting feature to use this.");
            }
        }

        /// <summary>
        /// Indicates whether a remoting connection is ready to be started using
        /// <see cref="StartConnectingToPlayer"/> or <see cref="StartListeningForPlayer"/>.
        /// </summary>
        public static bool IsReadyToStart
        {
            get => AppRemotingSubsystem.GetCurrent().IsReadyToStart();
        }

        /// <summary>
        /// Provides information on the current remoting session, if one exists.
        /// </summary>
        /// <param name="connectionState">The current connection state of the remote session.</param>
        /// <param name="disconnectReason">If the connection state is disconnected, this helps explain why.</param>
        /// <returns>Whether the information was successfully retrieved.</returns>
        public static bool TryGetConnectionState(out ConnectionState connectionState, out DisconnectReason disconnectReason)
        {
            return AppRemotingSubsystem.GetCurrent().TryGetConnectionState(out connectionState, out disconnectReason);
        }

        /// <summary>
        ///  To locate the `XR_REMOTING_REFERENCE_SPACE_TYPE_USER_MSFT` reference space in Unity's scene origin space in the remote app. For more details, reference the
        ///  <see href="https://docs.microsoft.com/windows/mixed-reality/develop/native/holographic-remoting-coordinate-system-synchronization-openxr">Coordinate System Synchronization with Holographic Remoting</see>.
        /// </summary>
        /// <param name="frameTime">Specify the <see cref="FrameTime"/> to locate the user reference space.</param>
        /// <param name="pose">Output the pose of the user reference space in the Unity's scene origin space.</param>
        /// <returns>Returns true when the user reference space is tracking and output pose is valid to be used.
        ///  Returns false when the user reference space lost tracking or it's not properly set up.</returns>
        public static bool TryLocateUserReferenceSpace(FrameTime frameTime, out Pose pose)
        {
            return AppRemotingSubsystem.GetCurrent().TryLocateUserReferenceSpace(frameTime, out pose);
        }

        /// <summary>
        /// Convert the time from a player app QPC time to the synchronized remote app QPC time.
        /// </summary>
        /// <param name="playerPerformanceCount">The performance count obtained in the player app using QueryPerformanceCounter.</param>
        /// <param name="remotePerformanceCount">Output the synchronized performance count as if using QueryPerformanceCounter in the remote app at the same time.
        ///     The output will be 0, indicating invalid time, if the function returns false.</param>
        /// <returns>Returns true when the time are successfully converted. 
        ///  Returns false indicates the time synchronization between remote and player app is not yet established.
        /// </returns>
        internal static bool TryConvertToRemoteTime(long playerPerformanceCount, out long remotePerformanceCount)
        {
            return AppRemotingSubsystem.GetCurrent().TryConvertToRemoteTime(playerPerformanceCount, out remotePerformanceCount);
        }

        /// <summary>
        /// Convert the time from a remote app QPC time to the synchronized player app QPC time.
        /// </summary>
        /// <param name="remotePerformanceCount">The performance count obtained in the remote app using QueryPerformanceCounter.</param>
        /// <param name="playerPerformanceCount">Output the synchronized performance count as if using QueryPerformanceCounter in the player app at the same time.
        ///     The output will be 0, indicating invalid time, if the function returns false.</param>
        /// <returns>Returns true when the time are successfully converted. 
        ///  Returns false indicates the time synchronization between remote and player app is not yet established.
        /// </returns>
        internal static bool TryConvertToPlayerTime(long remotePerformanceCount, out long playerPerformanceCount)
        {
            return AppRemotingSubsystem.GetCurrent().TryConvertToPlayerTime(remotePerformanceCount, out playerPerformanceCount);
        }

        /// <summary>
        /// Event triggered when <see cref="IsReadyToStart"/> changes from false to true.
        /// </summary>
        /// <remarks>
        /// Typically, applications can use this event to re-enable UX allowing the user to start a new remoting connection, as this  
        /// event indicates previous remoting sessions have fully completed and AppRemoting is ready for a new connection to start.
        /// </remarks>
        public static event ReadyToStartDelegate ReadyToStart
        {
            add
            {
                AppRemotingSubsystem.GetCurrent().ReadyToStart += value;
            }
            remove
            {
                AppRemotingSubsystem.GetCurrent().ReadyToStart -= value;
            }
        }

        /// <summary>
        /// Event triggered when the connection between remote app (Client/Server) and player is successfully established.
        /// </summary>
        public static event ConnectedDelegate Connected
        {
            add
            {
                AppRemotingSubsystem.GetCurrent().Connected += value;
            }
            remove
            {
                AppRemotingSubsystem.GetCurrent().Connected -= value;
            }
        }

        /// <summary>
        /// Event triggered when the connection between remote app (Client/Server) and player is disconnecting.
        /// </summary>
        /// <remarks>
        /// This event might be triggered several times during the `StartListeningForPlayer` coroutine. 
        /// This may also be triggered without a corresponding "Connected" event. 
        /// </remarks>
        public static event DisconnectingDelegate Disconnecting
        {
            add
            {
                AppRemotingSubsystem.GetCurrent().Disconnecting += value;
            }
            remove
            {
                AppRemotingSubsystem.GetCurrent().Disconnecting -= value;
            }
        }
    }

    /// <summary>
    /// Describes the event handler that can be implemented by remote app to get notified when <see cref="IsReadyToStart"/> changes from false to true.
    /// </summary>
    public delegate void ReadyToStartDelegate();

    /// <summary>
    /// Describes the event handler that can be implemented by remote app to get notified on a <see cref="Connected"/> event.
    /// </summary>
    public delegate void ConnectedDelegate();

    /// <summary>
    /// Describes the event handler that can be implemented by remote app to get notified on a <see cref="Disconnecting"/> event.
    /// </summary>
    /// <param name="disconnectReason">The reason for disconnecting</param>
    public delegate void DisconnectingDelegate(DisconnectReason disconnectReason);

    /// <summary>
    /// Describes the preferred video codec to use for the connection.
    /// </summary>
    public enum RemotingVideoCodec
    {
        /// <summary>
        /// Represents HEVC video codec preferred, fall back to H264 if HEVC is not supported by all participants.
        /// </summary>
        Auto = 0,
        /// <summary>
        /// Represents HEVC video codec.
        /// </summary>
        H265,
        /// <summary>
        /// Represents H264 video codec.
        /// </summary>
        H264,
    }

    /// <summary>
    /// Specifies the configuration for the remote app (Client) to initiate a remoting connection.
    /// </summary>
    [Serializable, StructLayout(LayoutKind.Sequential, Pack = 8)]
    [Obsolete("This struct is obsolete. Use 'RemotingConnectConfiguration' instead.", false)]
    public struct RemotingConfiguration
    {
        /// <summary>
        /// The host name or IP address of the player running in network server mode to connect to.
        /// </summary>
        public string RemoteHostName;

        /// <summary>
        /// The port number of the server's handshake port.
        /// </summary>
        public ushort RemotePort;

        /// <summary>
        /// The max bitrate in Kbps to use for the connection.
        /// </summary>
        public uint MaxBitrateKbps;

        /// <summary>
        /// The video codec to use for the connection.
        /// </summary>
        public RemotingVideoCodec VideoCodec;

        /// <summary>
        /// Enable/disable audio remoting.
        /// </summary>
        public bool EnableAudio;
    }

    /// <summary>
    /// Specifies the configuration for the remote app (Client) to initiate a remoting connection.
    /// </summary>
    [Serializable, StructLayout(LayoutKind.Sequential, Pack = 8)]
    public struct RemotingConnectConfiguration
    {
        /// <summary>
        /// The host name or IP address of the player running in network server mode to connect to.
        /// </summary>
        public string RemoteHostName;

        /// <summary>
        /// The port number of the server's handshake port.
        /// </summary>
        public ushort RemotePort;

        /// <summary>
        /// The max bitrate in Kbps to use for the connection.
        /// </summary>
        public uint MaxBitrateKbps;

        /// <summary>
        /// The video codec to use for the connection.
        /// </summary>
        public RemotingVideoCodec VideoCodec;

        /// <summary>
        /// Enable/disable audio remoting.
        /// </summary>
        public bool EnableAudio;

        /// <summary>
        /// Configuration to enable secure connection.
        /// </summary>
        public SecureRemotingConnectConfiguration? secureConnectConfiguration;
    }

    /// <summary>
    /// Specifies the configuration for the remote app (Client) to initiate a secure remoting connection.
    /// </summary>
    public struct SecureRemotingConnectConfiguration
    {
        ///<summary>
        /// Shared token between the remote app (Client) and remote player (Server). 
        /// Used by remote player (Server) to validate the remote app (Client) 
        /// before establishing a secure connection. For more details, reference the 
        /// <see href=" https://docs.microsoft.com/windows/mixed-reality/develop/native/holographic-remoting-secure-connection#planning-the-client-to-server-authentication">client to server authentication</see>.
        ///</summary>
        public string AuthenticationToken;

        ///<summary>
        /// Specify whether to request platform's default validation on the remote player's certificate using
        /// the validation functions of the underlying operating system or cryptography library.This bool is 
        /// taken into account only when <see cref="AppRemoting.ValidateServerCertificate"/> callback is implemented 
        /// by the remote app (Client). Otherwise, if the callback is not provided system validation is performed 
        /// regardless and is used for validating the remoting player's(Server) certificate.
        ///</summary>
        public bool PerformSystemValidation;

        /// <summary>
        /// The callback function to validate the certificate chain provided by the remote player (Server).
        /// </summary>
        public SecureRemotingValidateServerCertificateDelegate ValidateServerCertificateCallback;
    }

    /// <summary>
    /// Defines the result of a certificate validation for secure remoting connection.
    /// </summary>
    [Serializable, StructLayout(LayoutKind.Sequential, Pack = 8)]
    public struct SecureRemotingCertificateValidationResult
    {
        ///<summary>
        /// Specifies whether the certificate can be traced back to a trusted root
        ///</summary>
        public bool TrustedRoot;

        ///<summary>
        /// Specifies whether the certificate has been revoked
        ///</summary>
        public bool Revoked;

        ///<summary>
        /// Specifies whether the certificate is outside of its validity period (expired or not yet valid)
        ///</summary>
        public bool Expired;

        ///<summary>
        /// Specifies whether the allowed certificate usage is not compatible with its actual usage
        ///</summary>
        public bool WrongUsage;

        ///<summary>
        /// Specifies whether the revocation check failed
        ///</summary>
        public bool RevocationCheckFailed;

        ///<summary>
        /// Specifies whether the certificate to validate and/or certificate(s) in the certificate chain 
        /// contained invalid data and could not be examined
        ///</summary>
        public bool InvalidCertOrChain;

        ///<summary>
        /// Specifies the result of name validation, if there is a name mismatch between
        /// the name of the host presenting the certificate and the certificate subject
        ///</summary>
        public SecureRemotingNameValidationResult NameValidationResult;
    }

    /// <summary>
    /// Describes whether the name of the host presenting the certificate does not match the certificate subject.
    /// </summary>
    public enum SecureRemotingNameValidationResult
    {
        ///<summary>
        /// Represents that the name match cannot be reliably determined.
        ///</summary>
        ResultIndeterminate = 0,

        ///<summary>
        /// Represents the name of the host presenting the certificate matches the certificate subject.
        ///</summary>
        ResultMatch = 1,

        ///<summary>
        /// Represents the name of the host presenting the certificate does not match the certificate subject.
        ///</summary>
        ResultMismatch = 2,
    }

    /// <summary>
    /// Defines the callback that can be provided by the remote app (Client) for custom validation of remote player's(Server) certificate chain.
    /// </summary>
    /// <param name="hostName">The name of the host the connection is being established with</param>
    /// <param name="serverCertificateChain">The certificate chain that the server provides when initiating the secure connection</param>
    /// <param name="systemValidationResult">The result of system validation is provided by remoting runtime if system validation is requested by the remote app</param>
    /// <returns> Returns custom server certificated validation result.</returns>
    /// <remarks> System validation (as the name suggests) is certificate validation based on the underlying system’s cryptographic APIs and certificate stores. 
    /// Thus, results can vary depending on OS and local setup. The system validators are implemented in libbasix, which is owned by the RDV project.
    /// System validation in this API will forward to libbasix’ default validators for the respective platform
    /// </remarks>
    public delegate SecureRemotingCertificateValidationResult SecureRemotingValidateServerCertificateDelegate(string hostName, 
                                                                                                        X509Certificate2Collection serverCertificateChain, 
                                                                                                        SecureRemotingCertificateValidationResult? systemValidationResult = null);

    /// <summary>
    /// Specifies the configuration for the remote app (Server) to initiate a remoting connection in listen mode.
    /// </summary>
    [Serializable, StructLayout(LayoutKind.Sequential, Pack = 8)]
    public struct RemotingListenConfiguration
    {
        /// <summary>
        /// The host name or IP address of the player running in network server mode to connect to.
        /// </summary>
        public string ListenInterface;

        /// <summary>
        /// The port number of the server's handshake port.
        /// </summary>
        public ushort HandshakeListenPort;

        /// <summary>
        /// The port number of the server's transport port.
        /// </summary>
        public ushort TransportListenPort;

        /// <summary>
        /// The max bitrate in Kbps to use for the connection.
        /// </summary>
        public uint MaxBitrateKbps;

        /// <summary>
        /// The video codec to use for the connection.
        /// </summary>
        public RemotingVideoCodec VideoCodec;

        /// <summary>
        /// Enable/disable audio remoting.
        /// </summary>
        public bool EnableAudio;

        /// <summary>
        /// Configuration to enable secure connection.
        /// </summary>
        public SecureRemotingListenConfiguration? secureListenConfiguration;
    }

    /// <summary>
    /// Specifies the configuration for the remote app (Server) to initiate a secure remoting connection in linsten mode.
    /// </summary>
    public struct SecureRemotingListenConfiguration
    {
        /// <summary>
        /// Byte array containing a certificate store in PKCS#12 format. This store must contain the server
        /// certificate and the associated private key; optionally, it can also contain the certificate chain for the server certificate.
        /// </summary>
        public NativeArray<byte> Certificate;

        /// <summary>
        /// The name of the server certificate which is used to identify it in the
        /// certificate store. This is usually either the subject common name, or a friendly name assigned to the certificate.
        /// </summary>
        public string SubjectName;

        /// <summary>
        /// The passphrase needed to decrypt the private key. Can be an empty string
        /// if the private key is not encrypted. 
        /// </summary>
        /// <remarks>
        /// The passphrase is passed on as UTF-8 encoded and thus, depending on the encoding used when 
        /// writing the certificate store, passphrases containing characters beyond the 7-bit ASCII range may not work as expected.
        /// </remarks>
        public string KeyPassphrase;

        /// <summary>
        /// The callback function to validate authentication token provided by the remoting player (Client).
        ///</summary>
        public SecureRemotingValidateAuthenticationTokenDelegate ValidateAuthenticationTokenCallback;
    }

    /// <summary>
    /// The callback that needs to be implemented by remote app (Server) in secure listen mode 
    /// to validate remote player's (Client) authentication token.
    /// </summary>
    /// <param name="authenticationTokenToCheck">shared secret between the client and server. 
    /// Used to validate the remote player to establish a secure connection.</param>
    /// <returns> Returns true if the token validation succeeds and false if not.</returns>
    public delegate bool SecureRemotingValidateAuthenticationTokenDelegate(string authenticationTokenToCheck);

    /// <summary>
    /// Describes the current connection state.
    /// </summary>
    public enum ConnectionState
    {
        /// <summary>
        /// Represents that the state is not connected, and no connection attempt is
        /// in progress (Client), or not listening for incoming connections (Server).
        /// </summary>
        Disconnected = 0,
        /// <summary>
        /// Represents connecting to server (Client), listening for incoming
        /// connections (Server), or performing connection handshake (Client/Server).
        /// </summary>
        Connecting = 1,
        /// <summary>
        /// Represents fully connected, all communication channels established (Client/Server).
        /// </summary>
        Connected = 2,
    }

    /// <summary>
    /// Describes the reason for why the connection disconnected.
    /// </summary>
    public enum DisconnectReason
    {
        /// <summary>
        /// The connection succeeded and there was no connection failure.
        /// </summary>
        None = 0,
        /// <summary>
        /// The connection failed for an unknown reason.
        /// </summary>
        Unknown = 1,
        /// <summary>
        /// The secure connection was enabled, but certificate was missing, invalid, or not usable (Server).
        /// </summary>
        NoServerCertificate = 2,
        /// <summary>
        /// The handshake port could not be opened for accepting connections (Server).
        /// </summary>
        HandshakePortBusy = 3,
        /// <summary>
        /// The handshake server is unreachable (Client).
        /// </summary>
        HandshakeUnreachable = 4,
        /// <summary>
        /// The handshake server closed the connection prematurely; likely due to TLS/Plain mismatch or invalid certificate (Client).
        /// </summary>
        HandshakeConnectionFailed = 5,
        /// <summary>
        /// The authentication with the handshake server failed (Client).
        /// </summary>
        AuthenticationFailed = 6,
        /// <summary>
        /// No common compatible remoting version could be determined during handshake (Client).
        /// </summary>
        RemotingVersionMismatch = 7,
        /// <summary>
        /// No common transport protocol could be determined during handshake (Client).
        /// </summary>
        IncompatibleTransportProtocols = 8,
        /// <summary>
        /// The handshake failed for any other reason (Client).
        /// </summary>
        HandshakeFailed = 9,
        /// <summary>
        /// The transport port could not be opened for accepting connections (Server).
        /// </summary>
        TransportPortBusy = 10,
        /// <summary>
        /// The transport server is unreachable (Client).
        /// </summary>
        TransportUnreachable = 11,
        /// <summary>
        /// The transport connection was closed before all communication channels had been set up (Client/Server).
        /// </summary>
        TransportConnectionFailed = 12,
        /// <summary>
        /// The transport connection was closed due to protocol version mismatch (Client/Server).
        /// </summary>
        ProtocolVersionMismatch = 13,
        /// <summary>
        /// A protocol error occurred that was severe enough to invalidate the current connection or connection attempt (Client/Server).
        /// </summary>
        ProtocolError = 14,
        /// <summary>
        /// The transport connection was closed due to the requested video codec not being available (Client/Server).
        /// </summary>
        VideoCodecNotAvailable = 15,
        /// <summary>
        /// The connection attempt has been canceled (Client/Server).
        /// </summary>
        Canceled = 16,
        /// <summary>
        /// The connection has been closed by peer (Client/Server).
        /// </summary>
        ConnectionLost = 17,
        /// <summary>
        /// The connection has been closed due to graphics device loss (Client/Server).
        /// </summary>
        DeviceLost = 18,
        /// <summary>
        /// The connection has been closed by request (Client/Server).
        /// </summary>
        DisconnectRequest = 19,
        /// <summary>
        /// The network is unreachable. This usually means the client knows no route to reach the remote host (Client).
        /// </summary>
        HandshakeNetworkUnreachable = 20,
        /// <summary>
        /// No connection could be made because the remote side actively refused it. Usually this means that no host application is running (Client).
        /// </summary>
        HandshakeConnectionRefused = 21,
        /// <summary>
        /// The transport connection was closed due to the requested video format not being available (Client/Server).
        /// </summary>
        VideoFormatNotAvailable = 22,
        /// <summary>
        /// Disconnected after receiving a disconnect request from the peer (Client/Server).
        /// </summary>
        PeerDisconnectRequest = 23,
        /// <summary>
        /// Timed out while waiting for peer to close connection (Client/Server).
        /// </summary>
        PeerDisconnectTimeout = 24,
        /// <summary>
        /// Timed out while waiting for transport session to be opened (Client/Server).
        /// </summary>
        SessionOpenTimeout = 25,
        /// <summary>
        /// Timed out while waiting for the remoting handshake to complete (Client/Server).
        /// </summary>
        RemotingHandshakeTimeout = 26,
        /// <summary>
        /// The connection failed due to an internal error (Client/Server).
        /// </summary>
        InternalError = 27,
        /// <summary>
        /// The handshake could not be opened due to insufficient permissions (Client).
        /// </summary>
        HandshakePermissionDenied = 28,
    }
}
