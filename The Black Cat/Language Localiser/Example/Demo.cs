using TMPro;
using UnityEngine;

namespace BlackCatLocalisation
{
    public class Demo : MonoBehaviour
    {
        [SerializeField] private string languageId = "en_gb";
        [SerializeField] private TMP_Text tmp1;
        [SerializeField] private TMP_Text tmp2;
        [SerializeField] private TMP_Text tmp3;

        private int pressE = 0, pressZ = 0;

        private void Awake()
        {
            TextManager.SetLanguage(languageId);
        }

        private void Start()
        {
            tmp1.text = Text.Translatable("THIS_IS_ID_1");
            tmp2.text = Text.Translatable("THIS_IS_ID_2");
            tmp3.text = Text.Translatable("SUBS_ID_1", pressE, pressZ);
        }

        private void Update()
        {
            if (Input.GetKeyDown(KeyCode.E))
            {
                pressE++;
                tmp3.text = Text.Translatable("SUBS_ID_1", pressE, pressZ);
            }
            if (Input.GetKeyDown(KeyCode.Z))
            {
                pressZ++;
                tmp3.text = Text.Translatable("SUBS_ID_1", pressE, pressZ);
            }
        }
    }
}