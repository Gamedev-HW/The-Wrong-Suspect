using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using WrongSuspect.Events;
using WrongSuspect.Investigation;

namespace WrongSuspect.UI
{
    /// <summary>Shows the alibi cards when the preparation ends and reports the chosen one (0 = first card).</summary>
    public class AlibiView : MonoBehaviour
    {
        [Header("Listens to")]
        [SerializeField] private AlibiRequestEvent alibiRequested;

        [Header("Raises")]
        [SerializeField] private IntEvent alibiChosen;

        [Header("Parts")]
        [SerializeField] private GameObject panel;
        [SerializeField] private Button cardTemplate;

        private readonly List<Button> cards = new List<Button>();

        private void Awake()
        {
            cardTemplate.gameObject.SetActive(false);
            panel.SetActive(false);
        }

        private void OnEnable()
        {
            if (alibiRequested != null) alibiRequested.Raised += Show;
        }

        private void OnDisable()
        {
            if (alibiRequested != null) alibiRequested.Raised -= Show;
        }

        private void Show(string[] options)
        {
            for (int i = 0; i < options.Length; i++)
            {
                int index = i;
                Button card = Instantiate(cardTemplate, cardTemplate.transform.parent);
                card.gameObject.SetActive(true);
                card.GetComponentInChildren<Text>().text = options[i];
                card.onClick.AddListener(() => Choose(index));
                cards.Add(card);
            }
            panel.SetActive(true);
        }

        private void Choose(int index)
        {
            foreach (Button card in cards)
            {
                Destroy(card.gameObject);
            }
            cards.Clear();
            panel.SetActive(false);
            alibiChosen.Raise(index);
        }
    }
}
