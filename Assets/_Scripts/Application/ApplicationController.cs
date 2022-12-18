using Boxes.Connection;
using Boxes.Connection.Methods;
using Boxes.Connection.Services;
using Boxes.Connection.Services.Lobbies;
using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.SceneManagement;
using VContainer;
using VContainer.Unity;

namespace Boxes.ApplicationLifecycle
{
    public class ApplicationController : LifetimeScope
    {
        [SerializeField] ConnectionManager m_ConnectionManager;
        [SerializeField] NetworkManager m_NetworkManager;

        /// <summary>
        /// An entry point to the application, where we bind all the common dependencies to the root DI scope.
        /// </summary>
        protected override void Configure(IContainerBuilder builder)
        {
            base.Configure(builder);
            builder.RegisterComponent(m_ConnectionManager);
            builder.RegisterComponent(m_NetworkManager);

            builder.Register<LocalLobbyUser>(Lifetime.Singleton);
            builder.Register<LocalLobby>(Lifetime.Singleton);

            builder.Register<ProfileManager>(Lifetime.Singleton);

            builder.Register<AuthenticationFacade>(Lifetime.Singleton); //a manager entity that allows us to do anonymous authentication with unity services

            //LobbyServiceFacade is registered as entrypoint because it wants a callback after container is built to do it's initialization
            builder.RegisterEntryPoint<LobbyFacade>(Lifetime.Singleton).AsSelf();
        }

        private void Start()
        {
            DontDestroyOnLoad(gameObject);
            Application.targetFrameRate = 120;
            SceneManager.LoadScene("MainMenu");
        }


        protected override void OnDestroy()
        {
            base.OnDestroy();
        }
    }
}