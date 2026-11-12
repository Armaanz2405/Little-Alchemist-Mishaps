using System;
using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;
using Nocturne.UI;

namespace Nocturne.PotionMaking.Minigames
{
    public abstract class Minigame : MonoBehaviour
    {
        public UnityEvent<float> onFinished = new UnityEvent<float>();

        public abstract string Title { get; }
        public abstract string Instructions { get; }
        public bool Running { get; private set; }
        // score of the last finished round, 0..1, so stations can read it after onFinished
        public float LastScore { get; private set; } = 1f;

        protected RectTransform Root { get; private set; }
        protected TMP_Text InstructionLabel { get; private set; }
        private Action<float> callback;
        private Action cancelCallback;
        private bool finishing;
        private bool outOfEnergy;
        private int beginFrame;
        private float beganAt;
        private RectTransform leaveHint, leaveBox;
        private TMP_Text leaveKey;
        private string leaveLabel;

        // Leaving is for backing out of something opened by mistake, not for retrying a run that's going badly.
        // Games with a screen before the action (picking a tag, checking the mortar) say when that ends.
        protected virtual bool CanLeave => Time.time - beganAt < 2.5f;

        public void Begin(Action<float> onDone = null, bool outOfEnergy = false, Action onCancel = null)
        {
            if (Running) return;
            callback = onDone;
            cancelCallback = onCancel;
            finishing = false;
            this.outOfEnergy = outOfEnergy;
            beginFrame = Time.frameCount;
            beganAt = Time.time;
            var canvas = MinigameUI.MakeCanvas(transform, Title, 80);
            Root = MinigameUI.Rect(canvas.transform, "Panel", Vector2.zero, new Vector2(1100, 760), MinigameUI.Panel);
            UIStyle.Box(Root.GetComponent<Image>(), BoxKind.Panel);
            MinigameUI.Label(Root, $"<b>{Title}</b>", new Vector2(0, 330), 46, MinigameUI.Gold);
            InstructionLabel = MinigameUI.Label(Root, Instructions, new Vector2(0, 278), 24, MinigameUI.Ink);
            if (outOfEnergy) MinigameUI.Label(Root, "Out of energy, this one scores 0%", new Vector2(0, 240), 24, MinigameUI.Bad);
            Build();
            // some games only know what to say once they're built, like whether there's a tag to pick first
            InstructionLabel.text = Instructions;
            // the way out sits in the same corner on every minigame and goes away once leaving isn't possible
            leaveHint = MinigameUI.Row(Root, "Leave", new Vector2(526, 338), 8f, 1f);
            leaveLabel = MinigameInput.Label(MinigameInput.Hint.Cancel);
            leaveKey = UIStyle.Key(leaveHint, leaveLabel, 30f);
            leaveBox = MinigameUI.IconBox(leaveHint, MinigameUI.Icon.Cross, Vector2.zero, 34f, MinigameUI.Bad, true);
            MinigameUI.Settle(leaveHint);
            Running = true;
        }

        // leaves without a score: Esc (B on a pad) from inside the game, or whoever started it
        public void Cancel()
        {
            if (!Running || finishing) return;
            Running = false;
            Destroy(Root.parent.gameObject);
            OnCancelled();
            var onCancel = cancelCallback;
            cancelCallback = null;
            onCancel?.Invoke();
        }

        protected virtual void OnCancelled() { }

        // return true when the game used the cancel press for something else, like backing out of a sub-screen
        protected virtual bool HandleCancel() => false;

        protected abstract void Build();
        protected abstract void Step(float dt);

        private void Update()
        {
            // skip the frame the station started us on, so the key that opened the game isn't also its first input
            if (!Running || finishing || Time.frameCount <= beginFrame) return;
            bool canLeave = CanLeave;
            if (leaveHint.gameObject.activeSelf != canLeave) leaveHint.gameObject.SetActive(canLeave);
            string label = MinigameInput.Label(MinigameInput.Hint.Cancel);
            if (label != leaveLabel)
            {
                leaveLabel = label;
                UIStyle.SetKey(leaveKey, label);
                MinigameUI.Settle(leaveHint);
            }
            bool clickedOut = canLeave && MinigameInput.Clicked(out var at) && MinigameUI.Contains(leaveBox, at);
            if (clickedOut || (MinigameInput.CancelPressed && !HandleCancel() && canLeave))
            {
                Cancel();
                return;
            }
            Step(Time.deltaTime);
        }

        protected void Finish(float score, string result)
        {
            if (finishing) return;
            finishing = true;
            if (leaveHint != null) leaveHint.gameObject.SetActive(false);
            if (outOfEnergy)
            {
                score = 0f;
                result = "Out of energy";
            }
            LastScore = Mathf.Clamp01(score);
            StartCoroutine(FinishRoutine(LastScore, result));
        }

        private IEnumerator FinishRoutine(float score, string result)
        {
            MinigameUI.Label(Root, $"<b>{result}</b>\n<size=24>score {Mathf.RoundToInt(score * 100)}%</size>", new Vector2(0, -320), 36,
                score >= 0.5f ? MinigameUI.Good : MinigameUI.Bad);
            yield return new WaitForSeconds(1.5f);
            Running = false;
            Destroy(Root.parent.gameObject);
            callback?.Invoke(score);
            onFinished.Invoke(score);
        }
    }
}
