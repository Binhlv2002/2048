using DG.Tweening;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class Tile : MonoBehaviour
{
    public TileState state { get; private set; }
    public TileCell cell { get; private set; }
    public int number { get; private set; }

    private Image background;

    private TextMeshProUGUI text;

    public bool locked { get; set; }

    private void Awake()
    {
        background = GetComponent<Image>();
        text = GetComponentInChildren<TextMeshProUGUI>();
    }
    public void SetState(TileState state, int number)
    {
        this.state = state;
        this.number = number;
        background.color = state.backgroundColor;
        text.color = state.textColor;
        text.text = number.ToString();

    }

    public void Spawn(TileCell cell)
    {
        if (this.cell != null)
        {
            this.cell.tile = null;
        }

        this.cell = cell;
        this.cell.tile = this;

        transform.position = cell.transform.position;

        transform.localScale = Vector3.zero;
        transform.DOScale(Vector3.one, 0.1f).SetEase(Ease.OutBounce);
    }

    public void MoveTo(TileCell cell) 
    {
        if (this.cell != null)
        {
            this.cell.tile = null;
        }

        this.cell = cell;
        this.cell.tile = this;

        transform.DOMove(cell.transform.position, 0.1f).SetEase(Ease.OutQuad);
    }

    public void Merge(TileCell cell)
    {
        if (this.cell != null)
        {
            this.cell.tile = null;
        }

        this.cell = null;
        cell.tile.locked = true;
        transform.DOMove(cell.transform.position, 0.1f).SetEase(Ease.OutQuad).OnComplete(() => Destroy(gameObject));
    }
}
