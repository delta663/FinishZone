using System;
using System.Collections;
using System.Runtime.CompilerServices;
using BepInEx.Logging;
using BepInEx.Unity.IL2CPP.Utils.Collections;
using FinishZone.Services;
using ProjectM;
using ProjectM.Physics;
using ProjectM.Scripting;
using Unity.Entities;
using UnityEngine;

namespace FinishZone;

internal static class Core
{
    private static bool _hasInitialized;
    private static World _server;
    private static EntityManager _entityManager;
    private static ServerScriptMapper _serverScriptMapper;
    private static MonoBehaviour _monoBehaviour;

    public static World Server => _server ??= GetWorld("Server") ?? throw new Exception("There is no Server world (yet). Did you install a server mod on the client?");
    public static EntityManager EntityManager => _entityManager == default ? (_entityManager = Server.EntityManager) : _entityManager;
    public static ServerScriptMapper ServerScriptMapper => _serverScriptMapper ??= Server.GetExistingSystemManaged<ServerScriptMapper>();
    public static ServerGameManager ServerGameManager => ServerScriptMapper.GetServerGameManager();
    public static ManualLogSource Log => Plugin.PluginLog;
    public static FinishzoneService FinishzonesService { get; private set; }

    public static void LogException(System.Exception e, [CallerMemberName] string caller = null)
    {
        Log.LogError($"Failure in {caller}\nMessage: {e.Message} Inner:{e.InnerException?.Message}\n\nStack: {e.StackTrace}\nInner Stack: {e.InnerException?.StackTrace}");
    }

    internal static void InitializeAfterLoaded()
    {
        if (_hasInitialized)
            return;

        _server = GetWorld("Server") ?? throw new Exception("There is no Server world (yet). Did you install a server mod on the client?");
        _entityManager = _server.EntityManager;
        _serverScriptMapper = _server.GetExistingSystemManaged<ServerScriptMapper>();

        FinishzonesService = new FinishzoneService();
        FinishzonesService.Initialize();

        _hasInitialized = true;
        Log.LogInfo($"{nameof(InitializeAfterLoaded)} completed");
    }

    internal static World GetWorld(string name)
    {
        foreach (var world in World.s_AllWorlds)
        {
            if (world != null && world.Name == name)
                return world;
        }

        return null;
    }

    public static Coroutine StartCoroutine(IEnumerator routine)
    {
        if (_monoBehaviour == null)
        {
            var go = new GameObject("FinishZone");
            _monoBehaviour = go.AddComponent<IgnorePhysicsDebugSystem>();
            UnityEngine.Object.DontDestroyOnLoad(go);
        }

        return _monoBehaviour.StartCoroutine(routine.WrapToIl2Cpp());
    }

    public static void StopCoroutine(Coroutine coroutine)
    {
        if (_monoBehaviour == null)
            return;

        _monoBehaviour.StopCoroutine(coroutine);
    }
}
