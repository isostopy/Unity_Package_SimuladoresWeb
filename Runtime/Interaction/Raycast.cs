using System.Collections;
using UnityEngine;

public class Raycast : MonoBehaviour
{
    // VARIABLES ////////////////////////////////////////////////////////////////////////////////

    [Header("Range")]
    public float range = 50.0f;

    [Header("Pointer")]
    public GameObject teleportGizmo;

    private LineRenderer _line;

    private ISelectable _lastObject;
    private ISelectable _newObject;

    RaycastHit hit;

    // METHODS /////////////////////////////////////////////////////////////////////////////////

    private void Start()
    {
        if (gameObject.GetComponent<LineRenderer>())
        {
            _line = gameObject.GetComponent<LineRenderer>();
        }
    }

    /// <summary>
    /// Cast a ray in front of the Raycast GameObject.
    ///<para>
    ///	Returning the selectable that founds.
    ///	</para>
    /// </summary>
    /// <returns></returns>
    public ISelectable CheckSelection()
    {
        /* This function draws the line, places the teleport gizmo as needed,
		 * and calls the hovering functions of the selectable element. */

        // DRAW RAYCAST LINE //
        if (_line)
        {
            _line.SetPosition(0, transform.position);

            if (hit.collider)
            {
                _line.SetPosition(1, hit.point);
            }
            else
            {
                _line.SetPosition(1, transform.position + transform.forward * range);
            }
        }

        // CHECK HIT
        if (Physics.Raycast(gameObject.transform.position, gameObject.transform.forward, out hit, range))
        {
            var selection = hit.transform.gameObject.GetComponent<ISelectable>();

            //Debug.Log(selection);

            if (selection == null)
            {
                if (_lastObject != null) _lastObject.SelectionExit();
                _lastObject = null;

                if (teleportGizmo)
                {
                    teleportGizmo.SetActive(false);
                }

                return null;
            }
            else
            {
                _newObject = selection;

                // Place the teleport gizmo if we hitted a TELEPORT ISelectable.
                if (_newObject.gameObject.CompareTag("TeleportZone"))
                {
                    if (teleportGizmo)
                    {
                        if (!teleportGizmo.activeSelf)
                        {
                            teleportGizmo.SetActive(true);
                        }
                        teleportGizmo.transform.SetPositionAndRotation(hit.point, Quaternion.identity);

                    }
                }
                else
                {
                    if (teleportGizmo)
                    {
                        teleportGizmo.SetActive(false);
                    }
                }

                //Raycast Over Object.
                if (_newObject != _lastObject)
                {
                    _newObject.SelectionEnter();

                    if (_lastObject != null)
                    {
                        _lastObject.SelectionExit();
                    }
                    _lastObject = _newObject;
                }

                return selection;
            }
        }

        else
        {
            if (_newObject != null)
            {
                _newObject.SelectionExit();
            }

            if (_lastObject != null)
            {
                _lastObject.SelectionExit();
            }

            _newObject = null;
            _lastObject = null;

            if (teleportGizmo)
            {
                teleportGizmo.SetActive(false);
            }

            return null;
        }

    }
}
