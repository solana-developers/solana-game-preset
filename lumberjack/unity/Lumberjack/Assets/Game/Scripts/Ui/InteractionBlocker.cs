using TMPro;
using UnityEngine;

public class InteractionBlocker : MonoBehaviour
{
    public GameObject BlockingSpinner;
    public GameObject NonBlocking;
    public TextMeshProUGUI CurrentTransactionsInProgress;
    public TextMeshProUGUI LastTransactionTimeText;
    
    void Update()
    {
        BlockingSpinner.gameObject.SetActive(AnchorService.Instance.IsAnyBlockingTransactionInProgress);
        NonBlocking.gameObject.SetActive(AnchorService.Instance.IsAnyNonBlockingTransactionInProgress);
        CurrentTransactionsInProgress.text = (AnchorService.Instance.BlockingTransactionsInProgress +
                                             AnchorService.Instance.NonBlockingTransactionsInProgress).ToString();
        LastTransactionTimeText.text = $"Last took: {AnchorService.Instance.LastTransactionTimeInMs}ms";
    }
}
