using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using Unity.Services.Authentication;
using UnityEngine;

namespace Boxes.Connection.Services
{
    /// <summary>
    /// A facade infront of Unity's authentication service. Can be used to ensure the player is authenticated against 
    /// the Unity multiplayer services.
    /// </summary>
    public class AuthenticationFacade : MonoBehaviour
    {
        /*
         *  Will check to see if te player is authorized.
         *  If they are, return true.
         *  If they are not, attempt to anonymously sign in and then return true.
         *  Else log an error and return false.
         */
        public async Task<bool> EnsurePlayerIsAuthorized()
        {
            if (AuthenticationService.Instance.IsAuthorized)
            {
                return true;
            }

            try
            {
                await AuthenticationService.Instance.SignInAnonymouslyAsync();
                return true;
            }
            catch (AuthenticationException e)
            {
                var reason = $"{e.Message} ({e.InnerException?.Message})";
                //  m_UnityServiceErrorMessagePublisher.Publish(new UnityServiceErrorMessage("Authentication Error", reason, UnityServiceErrorMessage.Service.Authentication, e));
                //not rethrowing for authentication exceptions - any failure to authenticate is considered "handled failure"
                return false;
            }
            catch (Exception e)
            {
                //all other exceptions should still bubble up as unhandled ones
                var reason = $"{e.Message} ({e.InnerException?.Message})";
                // m_UnityServiceErrorMessagePublisher.Publish(new UnityServiceErrorMessage("Authentication Error", reason, UnityServiceErrorMessage.Service.Authentication, e));
                throw;
            }
        }
    }
}

