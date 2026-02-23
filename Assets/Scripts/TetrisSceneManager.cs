using System.Collections;
using UnityEngine;
using UnityEngine.UI;

class TetrisSceneManager : MonoBehaviour
{
    public Grid grid;
    public Block BlockPrefab;
    public BlockTemplate[] DropBlocks;
    public float DropSpeed = 1f;
    public float WaitGameOverSeconds = 3f;
    public Vector3Int DropStartPos = new Vector3Int(0, 11, 0);
    [SerializeField] int _score;
    public Text ScoreText;
    public Image NextBlockImage;
    public GameObject TitleUI, GameOverUI;
    Coroutine _currentCoroutine;

    public int Score
    {
        get
        {
            return _score;
        }

        set
        {
            _score = value;
            ScoreText.text = _score.ToString();
        }
    }
    
    void Start()
    {
        StartCoroutine(MainLoop());
    }

    public void StartTitle()
    {
        if (_currentCoroutine != null)
        {
            StopCoroutine(_currentCoroutine);
        }

        TitleUI.SetActive(true);
        ScoreText.transform.parent.parent.gameObject.SetActive(false);
        Score = 0;
        _currentCoroutine = StartCoroutine(TitleLoop());
    }

    public void GameOver()
    {
        if (_currentCoroutine != null)
        {
            StopCoroutine(_currentCoroutine);
        }

        GameOverUI.SetActive(true);
        ScoreText.transform.parent.parent.gameObject.SetActive(false);
        StartCoroutine(GameOverLoop());
    }

    IEnumerator GameOverLoop()
    {
        yield return new WaitForSeconds(WaitGameOverSeconds);
        GameOverUI.SetActive(false);
        StartTitle();
    }

    IEnumerator TitleLoop()
    {
        while (true)
        {
            yield return null;
        }
    }

    IEnumerator MainLoop()
    {
        grid.ClearMass();
        var nextTemplate = DropBlocks[Random.Range(0, DropBlocks.Length)];

        while (true)
        {
            yield return null;
            var block = Object.Instantiate(BlockPrefab, transform);
            block.SetTemplate(nextTemplate);
            block.Pos = DropStartPos;
            nextTemplate = DropBlocks[Random.Range(0, DropBlocks.Length)];
            NextBlockImage.sprite = nextTemplate.Thumbnail;

            while (!block.IsTouch(grid, Vector3Int.zero))
            {
                float t = 0;

                while (t < DropSpeed)
                {
                    t += Time.deltaTime;
                    Vector3Int offset = Vector3Int.zero;

                    if (Input.GetKeyDown(KeyCode.LeftArrow))
                    {
                        offset.x += 1;
                    }
                    else if (Input.GetKeyDown(KeyCode.RightArrow))
                    {
                        offset.x -= 1;
                    }

                    if (Input.GetKeyDown(KeyCode.DownArrow))
                    {
                        offset.z += 1;
                    }
                    else if (Input.GetKeyDown(KeyCode.UpArrow))
                    {
                        offset.z -= 1;
                    }

                    block.Move(grid, offset);

                    if (Input.GetKeyDown(KeyCode.Q))
                    {
                        block.RotateForward(grid, true);
                    }
                    else if (Input.GetKeyDown(KeyCode.E))
                    {
                        block.RotateForward(grid, false);
                    }

                    if (Input.GetKeyDown(KeyCode.A))
                    {
                        block.RotateUp(grid, true);
                    }
                    else if (Input.GetKeyDown(KeyCode.D))
                    {
                        block.RotateUp(grid, false);
                    }

                    yield return null;
                }

                block.DropMove(1);
            }

            var pos = block.Pos;
            pos.y++;
            block.Pos = pos;
            block.FillMass(grid);
            Object.Destroy(block.gameObject);

            if (block.MaxPos.y >= grid.Size.y)
            {
                GameOver();
                break;
            }
            else
            {
                CheckGridDepth();
            }
        }
    }

    void CheckGridDepth()
    {
        for (int y = 0; y < grid.Size.y; ++y)
        {
            if (grid.DoFillDepth(y))
            {
                grid.RemoveDepth(y);
                y--;
                Score += 100;
            }
        }
    }
}
