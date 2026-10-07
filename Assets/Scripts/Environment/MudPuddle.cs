using UnityEngine;
using BhootiyaRasta.Audio;
using BhootiyaRasta.Vehicle;

namespace BhootiyaRasta.Environment
{
    [RequireComponent(typeof(Collider))]
    public class MudPuddle : MonoBehaviour
    {
        private void OnTriggerEnter(Collider other)
        {
            var tractor = other.GetComponentInParent<TractorController>();
            if (tractor != null)
            {
                tractor.SetMudState(true);
                AudioManager.Instance?.PlayMudSquelch(other.transform.position);
            }
        }

        private void OnTriggerExit(Collider other)
        {
            var tractor = other.GetComponentInParent<TractorController>();
            if (tractor != null)
            {
                tractor.SetMudState(false);
            }
        }
    }
}
