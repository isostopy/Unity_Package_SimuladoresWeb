using UnityEngine;

/// <summary> Componente que hace que el GameObject que lo lleva mire a la MainCamera. </summary>
public class LookAtMainCamera : MonoBehaviour
{
	[SerializeField] bool inverse;
    Transform target = null;

	private void Start()
	{
		target = Camera.main.transform;	
	}

	void Update()
    {
		if (inverse)
		{
			transform.LookAt(transform.position * 2 - target.position, Vector2.up);
		}
		else
		{
            transform.LookAt(target, Vector2.up);
        }
    }
}
