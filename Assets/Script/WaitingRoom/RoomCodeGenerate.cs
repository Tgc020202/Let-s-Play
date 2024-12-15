// using UnityEngine;
// using UnityEngine.UI;

// public class RoomCodeGenerate : MonoBehaviour
// {
//     public Text roomCode;

//     void Start()
//     {
//         GenerateRoomCode();
//     }

//     void GenerateRoomCode()
//     {
//         int randomCode = Random.Range(1000, 10000);

//         if (VariableHolder.roomCode == null)
//         {
//             roomCode.text = randomCode.ToString();
//             VariableHolder.roomCode = roomCode.text;
//         }
//         else
//         {
//             roomCode.text = VariableHolder.roomCode;
//         }
//     }
// }
