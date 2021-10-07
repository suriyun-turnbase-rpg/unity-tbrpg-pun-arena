using Photon.Pun;
using PunArena.Enums;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using PunPlayer = Photon.Realtime.Player;

namespace PunArena.Battle
{
    public class PunArenaGameplayManager : GamePlayManager
    {
        public float decisionWaitingDuration = 10f;
        protected int loadedFormation = 0;
        protected readonly Dictionary<string, CharacterEntity> allCharacters = new Dictionary<string, CharacterEntity>();
        protected ERoomState currentState;
        protected Coroutine waitForActionCoroutine;
        public bool WaitingForAction
        {
            get { return waitForActionCoroutine != null; }
        }

        protected override void Awake()
        {
            // Clear helper, online gameplay manager not allow helper
            Helper = null;
            // Set battle type to arena
            BattleType = EBattleType.Arena;
            base.Awake();
            PunArenaManager.Instance.onRoomStateChange.AddListener(OnStateChange);
            PunArenaManager.Instance.onUpdateActiveCharacter += OnUpdateActiveCharacter;
            PunArenaManager.Instance.onDoSelectedAction += OnDoSelectedAction;
            PunArenaManager.Instance.onUpdateGameplayState += OnUpdateGameplayState;
            GameInstance.Singleton.onLoadSceneStart.AddListener(OnLoadSceneStart);
        }

        protected virtual void OnDestroy()
        {
            PunArenaManager.Instance.onRoomStateChange.RemoveListener(OnStateChange);
            PunArenaManager.Instance.onUpdateActiveCharacter -= OnUpdateActiveCharacter;
            PunArenaManager.Instance.onDoSelectedAction -= OnDoSelectedAction;
            GameInstance.Singleton.onLoadSceneStart.RemoveListener(OnLoadSceneStart);
        }

        protected override void SetupTeamAFormation()
        {
            teamAFormation.ClearCharacters();
            teamAFormation.foeFormation = teamBFormation;
            if (PhotonNetwork.LocalPlayer.GetTeam() == 0)
                CurrentTeamFormation = teamAFormation;
        }

        protected override void SetupTeamBFormation()
        {
            teamBFormation.ClearCharacters();
            teamBFormation.foeFormation = teamAFormation;
            if (PhotonNetwork.LocalPlayer.GetTeam() == 1)
                CurrentTeamFormation = teamBFormation;
        }

        protected override void Start()
        {
            foreach (var player in PhotonNetwork.CurrentRoom.Players.Values)
            {
                GetArenaFormationCharactersAndEquipmentsForPlayer(player);
            }
        }

        protected void GetArenaFormationCharactersAndEquipmentsForPlayer(PunPlayer player)
        {
            GameInstance.GameService.GetArenaFormationCharactersAndEquipments(player.UserId, (result) =>
            {
                GameInstance.Singleton.OnGameServiceGetFormationCharactersAndEquipments(result);
                if (player.GetTeam() == 0)
                {
                    teamAFormation.SetCharacters(result.characters.ToArray());
                    foreach (var character in teamAFormation.Characters.Values)
                    {
                        character.gameObject.AddComponent<PunArenaCharacterEntityActionOverride>();
                        allCharacters.Add(character.Item.id, character as CharacterEntity);
                    }
                }
                else if (player.GetTeam() == 1)
                {
                    teamBFormation.SetCharacters(result.characters.ToArray());
                    foreach (var character in teamBFormation.Characters.Values)
                    {
                        character.gameObject.AddComponent<PunArenaCharacterEntityActionOverride>();
                        allCharacters.Add(character.Item.id, character as CharacterEntity);
                    }
                }
                loadedFormation++;
                if (loadedFormation >= 2)
                {
                    // Tell the server that the this client is enter the game
                    PunArenaManager.Instance.SetPlayerEnterGameState();
                }
            });
        }

        private void OnStateChange(ERoomState state)
        {
            if (currentState != state)
            {
                currentState = state;
                switch (currentState)
                {
                    case ERoomState.Battle:
                        CurrentWave = 0;
                        StartCoroutine(OnStartBattleRoutine());
                        break;
                }
            }
        }

        private IEnumerator OnStartBattleRoutine()
        {
            yield return null;
            CurrentTeamFormation.MoveCharactersToFormation(false);
            CurrentTeamFormation.foeFormation.MoveCharactersToFormation(false);
            yield return new WaitForSeconds(moveToNextWaveDelay);
            NewTurn();
        }

        protected override void Update()
        {
            base.Update();
            // Fix timescale to 1
            Time.timeScale = 1;
        }

        public override void NewTurn()
        {
            if (!PhotonNetwork.IsMasterClient)
                return;
            UpdateActivatingCharacter();
            // Broadcast activate character
            PunArenaManager.Instance.SendUpdateActiveCharacter(ActiveCharacter.Item.Id);
        }

        private void OnUpdateActiveCharacter(string id)
        {
            ActiveCharacter = allCharacters[id];
            ActiveCharacter.DecreaseBuffsTurn();
            ActiveCharacter.DecreaseSkillsTurn();
            ActiveCharacter.ResetStates();
            if (ActiveCharacter.Hp > 0 &&
                !ActiveCharacter.IsStun)
            {
                if (ActiveCharacter.IsPlayerCharacter)
                {
                    if (IsAutoPlay)
                    {
                        ActiveCharacter.RandomAction();
                    }
                    else
                    {
                        uiCharacterActionManager.Show();
                        waitForActionCoroutine = StartCoroutine(WaitForActionSelection());
                    }
                }
                else
                {
                    if (PhotonNetwork.CurrentRoom.PlayerCount == 1)
                    {
                        // Another player exit the game
                        ActiveCharacter.RandomAction();
                    }
                    else
                    {
                        waitForActionCoroutine = StartCoroutine(WaitForActionSelection());
                    }
                }
            }
            else
            {
                ActiveCharacter.NotifyEndAction();
            }
        }

        private IEnumerator WaitForActionSelection()
        {
            yield return new WaitForSecondsRealtime(decisionWaitingDuration);
            // Time out, random action
            if (PhotonNetwork.IsMasterClient)
                ActiveCharacter.RandomAction();
        }

        public void OnDoSelectedAction(string entityId, string targetEntityId, int action, int seed)
        {
            if (waitForActionCoroutine != null)
            {
                StopCoroutine(waitForActionCoroutine);
                waitForActionCoroutine = null;
            }
            CharacterEntity character = allCharacters[entityId];
            CharacterEntity target = null;
            if (!string.IsNullOrEmpty(targetEntityId))
                target = allCharacters[targetEntityId];
            character.Action = action;
            character.ActionTarget = target;
            if (character.Action == CharacterEntity.ACTION_ATTACK)
                character.DoAttackAction(seed);
            else
                character.DoSkillAction(seed);
        }

        public void OnUpdateGameplayState(int winnerActorNumber, int loserActorNumber)
        {
            if (winnerActorNumber >= 0 || loserActorNumber >= 0)
            {
                isEnding = true;
                if (winnerActorNumber == PhotonNetwork.LocalPlayer.ActorNumber)
                {
                    // Show win dialog
                    uiWin.Show();
                }
                else
                {
                    // Show lose dialog
                    uiLose.Show();
                }
            }
        }

        public override void NotifyEndAction(CharacterEntity character)
        {
            if (!PhotonNetwork.IsMasterClient)
                return;

            if (character != ActiveCharacter)
                return;

            int winnerActorNumber = -1;
            int loserActorNumber = -1;
            if (!CurrentTeamFormation.IsAnyCharacterAlive())
            {
                // Manager lose
                ActiveCharacter = null;
                // Define loser
                loserActorNumber = PhotonNetwork.LocalPlayer.ActorNumber;
                // Find other session to define winner
                foreach (var actionNumber in PhotonNetwork.CurrentRoom.Players.Keys)
                {
                    if (actionNumber != PhotonNetwork.LocalPlayer.ActorNumber)
                    {
                        winnerActorNumber = actionNumber;
                        break;
                    }
                }
            }
            else if (!CurrentTeamFormation.foeFormation.IsAnyCharacterAlive())
            {
                // Manager win
                ActiveCharacter = null;
                // Define winner
                winnerActorNumber = PhotonNetwork.LocalPlayer.ActorNumber;
                // Find other session to define loser
                foreach (var actionNumber in PhotonNetwork.CurrentRoom.Players.Keys)
                {
                    if (actionNumber != PhotonNetwork.LocalPlayer.ActorNumber)
                    {
                        loserActorNumber = actionNumber;
                        break;
                    }
                }
            }
            else
            {
                // No winner yet.
                NewTurn();
            }

            // Send characters updating to server
            PunArenaManager.Instance.SendUpdateGameplayState(winnerActorNumber, loserActorNumber);
        }

        public override void NextWave()
        {
            // Override to do nothing
        }

        public override void OnRevive()
        {
            // Override to do nothing
        }

        public override void Restart()
        {
            // Override to do nothing
        }

        public override void Giveup(UnityAction onError)
        {
            GameInstance.Singleton.LoadManageScene();
        }

        public override void Revive(UnityAction onError)
        {
            // Override to do nothing
        }

        private void OnLoadSceneStart(string sceneName, float progress)
        {
            if (!sceneName.Equals(PunArenaManager.Instance.battleScene))
            {
                PhotonNetwork.LeaveRoom();
            }
        }
    }
}
