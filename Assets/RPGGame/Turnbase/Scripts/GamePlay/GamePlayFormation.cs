using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GamePlayFormation : BaseGamePlayFormation
{
    public GamePlayManager CastedManager { get { return BaseGamePlayManager.Singleton as GamePlayManager; } }
    public GamePlayFormation foeFormation;
    public readonly Dictionary<int, UICharacterStats> UIStats = new Dictionary<int, UICharacterStats>();

    private void Start()
    {
        if (SceneManager.GetActiveScene().name == GameInstance.Singleton.manageScene)
            SetFormationCharacters(BaseGamePlayManager.BattleType);
    }

    public override BaseCharacterEntity SetCharacter(int position, PlayerItem item)
    {
        var character = base.SetCharacter(position, item) as CharacterEntity;

        if (character == null)
            return null;

        UICharacterStats uiStats;
        if (UIStats.TryGetValue(position, out uiStats))
        {
            Destroy(uiStats.gameObject);
            UIStats.Remove(position);
        }

        if (CastedManager != null)
        {
            uiStats = Instantiate(CastedManager.uiCharacterStatsPrefab, CastedManager.uiCharacterStatsContainer);
            uiStats.transform.localScale = Vector3.one;
            uiStats.character = character;
            uiStats.Show();
            character.uiCharacterStats = uiStats;
        }

        return character;
    }

    public override BaseCharacterEntity SetHelperCharacter(PlayerItem item)
    {
        var character = base.SetHelperCharacter(item) as CharacterEntity;

        if (character == null)
            return null;

        var position = character.Position;

        UICharacterStats uiStats;
        if (UIStats.TryGetValue(position, out uiStats))
        {
            Destroy(uiStats.gameObject);
            UIStats.Remove(position);
        }

        if (CastedManager != null)
        {
            uiStats = Instantiate(CastedManager.uiCharacterStatsPrefab, CastedManager.uiCharacterStatsContainer);
            uiStats.transform.localScale = Vector3.one;
            uiStats.character = character;
            uiStats.Show();
            character.uiCharacterStats = uiStats;
        }

        return character;
    }

    public void Revive()
    {
        var characters = Characters.Values;
        foreach (var character in characters)
        {
            character.Revive();
        }
    }

    public bool IsAnyCharacterAlive()
    {
        var characters = Characters.Values;
        foreach (var character in characters)
        {
            if (character.Hp > 0)
                return true;
        }
        return false;
    }

    public bool TryGetHeadingToFoeRotation(out Quaternion rotation)
    {
        if (foeFormation != null)
        {
            var rotateHeading = foeFormation.transform.position - transform.position;
            rotation = Quaternion.LookRotation(rotateHeading);
            return true;
        }
        rotation = Quaternion.identity;
        return false;
    }

    public Coroutine MoveCharactersToFormation(bool stillForceMoving)
    {
        return StartCoroutine(MoveCharactersToFormationRoutine(stillForceMoving));
    }

    private IEnumerator MoveCharactersToFormationRoutine(bool stillForceMoving)
    {
        var characters = Characters.Values;
        foreach (var character in characters)
        {
            var castedCharacter = character as CharacterEntity;
            castedCharacter.forcePlayMoving = stillForceMoving;
            castedCharacter.MoveTo(character.Container.position, CastedManager.formationMoveSpeed);
        }
        while (true)
        {
            yield return 0;
            var ifEveryoneReachedTarget = true;
            foreach (var character in characters)
            {
                var castedCharacter = character as CharacterEntity;
                if (castedCharacter.IsMovingToTarget)
                {
                    ifEveryoneReachedTarget = false;
                    break;
                }
            }
            if (ifEveryoneReachedTarget)
                break;
        }
    }

    public void SetActiveDeadCharacters(bool isActive)
    {
        var characters = Characters.Values;
        foreach (var character in characters)
        {
            if (character.Hp <= 0)
                character.gameObject.SetActive(isActive);
        }
    }

    public Coroutine ForceCharactersPlayMoving(float duration)
    {
        return StartCoroutine(ForceCharactersPlayMovingRoutine(duration));
    }

    private IEnumerator ForceCharactersPlayMovingRoutine(float duration)
    {
        var characters = Characters.Values;
        foreach (var character in characters)
        {
            var castedCharacter = character as CharacterEntity;
            castedCharacter.forcePlayMoving = true;
        }
        yield return new WaitForSeconds(duration);
        foreach (var character in characters)
        {
            var castedCharacter = character as CharacterEntity;
            castedCharacter.forcePlayMoving = false;
        }
    }

    public void SetCharactersSelectable(bool selectable, bool deadCharacter = false)
    {
        var characters = Characters.Values;
        foreach (var character in characters)
        {
            var castedCharacter = character as CharacterEntity;
            if (deadCharacter && castedCharacter.Hp > 0)
            {
                castedCharacter.selectable = false;
                continue;
            }
            if (!deadCharacter && castedCharacter.Hp <= 0)
            {
                castedCharacter.selectable = false;
                continue;
            }
            castedCharacter.selectable = selectable;
        }
    }

    public int CountDeadCharacters()
    {
        return Characters.Values.Where(a => a.Hp <= 0).ToList().Count;
    }
}
