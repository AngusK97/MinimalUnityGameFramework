using System.Text;
using UnityEngine;
using UnityEngine.UI;
using System.Collections;

namespace UI.Views
{
    public class TypeWriter : MonoBehaviour
    {
        [SerializeField] private Text contentText;
        [SerializeField] private float normalInterval = 0.1f;
    
        public string extraDelayStartSign = "@";
        public string extraDelayEndSign = "#";
        
        private Coroutine _typewriterAnimCoroutine;
        private StringBuilder _curContentStringBuilder;
        private StringBuilder _extraDelayValueStringBuilder;

        public void Clear()
        {
            if (_typewriterAnimCoroutine != null)
            {
                StopCoroutine(_typewriterAnimCoroutine);
            }
        
            _curContentStringBuilder?.Clear();
            _extraDelayValueStringBuilder?.Clear();
            contentText.text = string.Empty;
        }

        public Coroutine ShowText(string rawContent)
        {
            if (_typewriterAnimCoroutine != null)
            {
                StopCoroutine(_typewriterAnimCoroutine);
            }

            _typewriterAnimCoroutine = StartCoroutine(PlayTypeWriterAnim(rawContent));
            return _typewriterAnimCoroutine;
        }
    
        private IEnumerator PlayTypeWriterAnim(string rawContent)
        {
            contentText.text = string.Empty;
        
            _curContentStringBuilder ??= new StringBuilder();
            _curContentStringBuilder.Clear();
        
            _extraDelayValueStringBuilder ??= new StringBuilder();
            _extraDelayValueStringBuilder.Clear();
        
            var readingExtraDelay = false;
        
            foreach (var curChar in rawContent)
            {
                // check is delay start sign
                if (extraDelayStartSign.Contains(curChar))
                {
                    readingExtraDelay = true;
                    continue;
                }

                // check is delay end sign
                if (extraDelayEndSign.Contains(curChar))
                {
                    readingExtraDelay = false;
                
                    if (_extraDelayValueStringBuilder.Length > 0)
                    {
                        var extraDelayValueStr = _extraDelayValueStringBuilder.ToString();
                        var extraDelayValue = float.Parse(extraDelayValueStr);
                        _extraDelayValueStringBuilder.Clear();
                        yield return new WaitForSeconds(extraDelayValue);
                    }
                
                    continue;
                }
            
                // read extra delay value
                if (readingExtraDelay)
                {
                    _extraDelayValueStringBuilder.Append(curChar);
                    continue;
                }
            
                _curContentStringBuilder.Append(curChar);
                contentText.text = _curContentStringBuilder.ToString();
                yield return new WaitForSeconds(normalInterval);
            }
        }
    }
}