using UnityEngine;


public class HideMannequin : MonoBehaviour
{

    [SerializeField] GameObject mannequin;

    void Start()
    {
        mannequin.SetActive(false);
    }

}
