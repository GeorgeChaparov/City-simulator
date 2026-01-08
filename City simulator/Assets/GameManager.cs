using System.Collections;
using UnityEngine;
public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    public int counter = 0;

    public bool m_Continue = false;
    public bool m_Skip = false;
    public bool m_Reset = false;

    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
        }
        else
        {
            Instance = this;
        }

        GridGlobals.Init();
    }

    void Update()
    {
        if (Input.GetKeyUp(KeyCode.N))
        {
            counter++;
        }

        if (Input.GetKeyUp(KeyCode.C))
        {
            m_Continue = true;
        }

        if (Input.GetKeyUp(KeyCode.S))
        {
            m_Skip = true;
            m_Continue = true;
        }

        if (Input.GetKeyUp(KeyCode.B))
        {
            m_Skip = false;
            m_Continue = false;
        }

        if (Input.GetKeyUp(KeyCode.R))
        {
            m_Reset = true;

            GameManager.Instance.m_Continue = false;
            GameManager.Instance.m_Skip = false;
            GameManager.Instance.counter = 0;
        }

        if (Input.GetKeyUp(KeyCode.D))
        {
            GridGlobals.PositionsToCheck = (0, GridGenerator.m_IMaskOffsets);
        }

        if (Input.GetKeyUp(KeyCode.F))
        {
            GridGlobals.PositionsToCheck = (0, GridGenerator.m_LMaskOffsets);
        }

        if (Input.GetKeyUp(KeyCode.G))
        {
            GridGlobals.PositionsToCheck = (0, GridGenerator.m_TMaskOffsets);
        }

        if (Input.GetKeyUp(KeyCode.H))
        {
            GridGlobals.PositionsToCheck = (0, GridGenerator.m_XMaskOffsets);
        }
    }
}
