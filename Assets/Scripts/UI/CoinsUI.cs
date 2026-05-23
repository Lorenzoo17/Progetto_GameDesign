using TMPro;
using UnityEngine;

public class CoinsUI : MonoBehaviour {
    [SerializeField] private TextMeshProUGUI mutagenCoins;
    [SerializeField] private TextMeshProUGUI dungeonCoins;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start() {
        if (MetaProgressionManager.Instance != null) {
            MetaProgressionManager.Instance.OnMutagenCoinChanged += Instance_OnMutagenCoinChanged;
            MetaProgressionManager.Instance.OnDungeonCoinChanged += Instance_OnDungeonCoinChanged;

            dungeonCoins.text = MetaProgressionManager.Instance.DungeonCoin.ToString();
            mutagenCoins.text = MetaProgressionManager.Instance.MutagenCoin.ToString();
        }
    }

    private void Instance_OnDungeonCoinChanged(object sender, System.EventArgs e) {
        if (MetaProgressionManager.Instance == null) return;

        dungeonCoins.text = MetaProgressionManager.Instance.DungeonCoin.ToString();
    }

    private void Instance_OnMutagenCoinChanged(object sender, System.EventArgs e) {
        if (MetaProgressionManager.Instance == null) return;

        mutagenCoins.text = MetaProgressionManager.Instance.MutagenCoin.ToString();
    }
}
