using UnityEngine;
using TMPro;

public class FPSCounter : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI _avgFPSText;
    [SerializeField] private TextMeshProUGUI _oneFPSLowText;
    [SerializeField] private TextMeshProUGUI _zeroOneFPSLowText;

    public float UpdateAVGTime;
    public float UpdateLowTime;

    private float _timeAVG = 0;
    private int _frameCount = 0;
    private int _avgFPS = 0;
    private float _timeLow = 0;
    private const int BUFFER_SIZE = 4096;
    private float[] _frameTimes = new float[BUFFER_SIZE];
    private int _frameTimesIndex = 0;
    private int _oneFPSLow = 0;
    private int _zeroOneFPSLow = 0;

    private void Awake()
    {
        for (int i = 0; i < BUFFER_SIZE; i++)
        {
            _frameTimes[i] = -1;
        }
    }

    private void Update()
    {
        UpdateValues();

        if (_timeAVG > UpdateAVGTime)
        {
            UpdateAVGText();
            ReinitializeAVG();
        }
        if (_timeLow > UpdateLowTime)
        {
            UpdateLowText();
            ReinitializeLow();
        }
    }

    private void UpdateValues()
    {
        _timeAVG += Time.unscaledDeltaTime;
        _frameCount++;

        _timeLow += Time.unscaledDeltaTime;
        InsertFrameTime(Time.unscaledDeltaTime);
    }

    private void InsertFrameTime(float frameTime)
    {
        _frameTimesIndex = 0;

        while(_frameTimesIndex < _frameTimes.Length && _frameTimes[_frameTimesIndex] >= frameTime)
        {
            _frameTimesIndex++;
        }

        if(_frameTimesIndex < _frameTimes.Length)
        {
            for(int i = _frameTimes.Length - 1; i > _frameTimesIndex; i--)
            {
                _frameTimes[i] = _frameTimes[i - 1];
            }
            _frameTimes[_frameTimesIndex] = frameTime;
        }
    }

    private void UpdateAVGText()
    {
        // Average
        _avgFPS = (int)Mathf.Floor(_frameCount / _timeAVG);

        // Text Update
        _avgFPSText.text = _avgFPS.ToString();
    }

    private void ReinitializeAVG()
    {
        _timeAVG = _timeAVG - UpdateAVGTime;
        _frameCount = 0;
    }

    private void UpdateLowText()
    {
        // %1 Low
        float sum = 0;
        int count = 0;
        for (int i = 0; i < BUFFER_SIZE / 100; i++)
        {
            if (_frameTimes[i] >= 0)
            {
                sum += 1 / _frameTimes[i];
                count++;
            }
        }
        _oneFPSLow = (int)Mathf.Floor(sum / count);

        // %0.1 Low
        sum = 0;
        count = 0;
        for (int i = 0; i < BUFFER_SIZE / 1000; i++)
        {
            if (_frameTimes[i] >= 0)
            {
                sum += 1 / _frameTimes[i];
                count++;
            }
        }
        _zeroOneFPSLow = (int)Mathf.Floor(sum / count);

        // Text Update
        _oneFPSLowText.text = _oneFPSLow.ToString();
        _zeroOneFPSLowText.text = _zeroOneFPSLow.ToString();
    }

    private void ReinitializeLow()
    {
        _timeLow = _timeLow - UpdateLowTime;
        for (int i = 0; i < BUFFER_SIZE; i++)
        {
            _frameTimes[i] = -1;
        }
    }

}
