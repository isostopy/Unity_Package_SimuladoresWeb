using Unity.Multiplayer.Center.Common;
using UnityEditor.UIElements;
using UnityEngine;

public class SampleManager : MonoBehaviour
{
    string dateStart;

    // Usadas para calcular el porcentaje de acierto, es decir la puntuación.
    int answeredRight;
    int questionCount;

    public void Start()
    {
        // Es necesario inicializar el sistema de evaluación. Y tener el archivo "evaluation_messages" en StreamingAssets
        StartCoroutine(EvaluationInfoManager.InitializeAsync());
        StartSimulation();
    }

    public void StartSimulation()
    {
        // Es necesario guardar la fecha hora de inicio de la simulación
        dateStart = System.DateTime.Now.ToString();
        Debug.Log(dateStart);
    }

    // Esto es un ejemplo ficticio que debe adaptarse al comportamiento y funcionalidad de cada simulador
    public void Answer(bool isCorrectAnswer, string questionID)
    {
        questionCount++;
        if (isCorrectAnswer)
        {
            answeredRight++;
        }
        else
        {
            // Esto añade el id a una lista de IDs que registra los fallos, cada id tiene un mensaje de Error asosciado en un JSON.
            EvaluationInfoManager.AddIdToList(questionID); 
        }
    }

    public void EndSimulation()
    {
        Answer(true, "sampleQuestion1");
        Answer(false, "sampleQuestion2");

        float percentage = (float)answeredRight / (float)questionCount;

        string json = EvaluationRecordManager.BuildJson("sample", dateStart, System.DateTime.Now.ToString(), (percentage * 100).ToString("00") + "%", EvaluationInfoManager.GetFullEvaluationLog());

        Debug.Log(json);

        EvaluationRecordManager.Instance.SendPost(EvaluationRecordManager.Instance.url, json);
    }

}
