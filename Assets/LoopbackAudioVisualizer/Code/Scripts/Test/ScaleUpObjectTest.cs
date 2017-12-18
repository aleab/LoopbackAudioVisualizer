using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Aleab.LoopbackAudioVisualizer.Scripts.Test
{
    public class ScaleUpObjectTest : MonoBehaviour
    {
        [SerializeField]
        [Range(-3.0f, 3.0f)]
        private float scale = 1.0f;

        private List<ScaleUpObject> objects;

        private void Awake()
        {
            this.objects = this.gameObject.GetComponentsInChildren<ScaleUpObject>(true).ToList();
        }

        private void Start()
        {
            this.StartCoroutine(this.UpdateScale());
        }

        private IEnumerator UpdateScale()
        {
            while (true)
            {
                foreach (var obj in this.objects)
                    obj.ScaleSmooth(this.scale);

                yield return new WaitForSeconds(0.05f);
            }
        }
    }
}