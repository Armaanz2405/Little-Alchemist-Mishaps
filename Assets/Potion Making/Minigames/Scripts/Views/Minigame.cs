using System;
using System.Collections;
using UnityEngine;
using UnityEngine.Events;

namespace Nocturne.PotionMaking.Minigames
{
    public abstract class Minigame : MonoBehaviour
    {
        public UnityEvent<float> onFinished = new UnityEvent<float>();

        public abstract string Title { get; }
        public abstract string Instructions { get; }
        public bool Running { get; private set; }

        protected RectTransform Root { get; private set; }
        private Action<float> callback;
        private bool finishing;

        public void Begin(Action<float> onDone = null)
        {
            if (Running) return;
            callback = onDone;
            finishing = false;
            if (Root != null) Destroy(Root.gameObject);
            var canvas = MinigameUI.MakeCanvas(transform, Title + "Canvas", 80);
            Root = MinigameUI.Rect(canvas.transform, "Panel", Vector2.zero, new Vector2(1100, 760), MinigameUI.Panel);
            MinigameUI.Text(Root, $"<b>{Title}</b>", new Vector2(0, 330), 46, MinigameUI.Gold);
            MinigameUI.Text(Root, Instructions, new Vector2(0, 272), 26, MinigameUI.Ink);
            Build();
            Running = true;
        }

        public void Cancel()
        {
            if (!Running) return;
            Running = false;
            if (Root != null) Destroy(Root.parent.gameObject);
        }

        protected abstract void Build();
        protected abstract void Step(float dt);

        private void Update()
        {
            if (Running && !finishing) Step(Time.deltaTime);
        }

        protected void Finish(float score, string resultText)
        {
            if (finishing) return;
            finishing = true;
            StartCoroutine(FinishRoutine(Mathf.Clamp01(score), resultText));
        }

        private IEnumerator FinishRoutine(float score, string resultText)
        {
            MinigameUI.Text(Root, $"<b>{resultText}</b>\n<size=70%>score {Mathf.RoundToInt(score * 100)}%</size>", new Vector2(0, -335), 34,
                score >= 0.5f ? MinigameUI.Good : MinigameUI.Bad);
            yield return new WaitForSeconds(1.4f);
            Running = false;
            if (Root != null) Destroy(Root.parent.gameObject);
            callback?.Invoke(score);
            onFinished.Invoke(score);
        }
    }
}
