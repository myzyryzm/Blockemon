using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
[RequireComponent(typeof(CapsuleCollider))]
[RequireComponent(typeof(TargetingRigidbody))]
public class CharacterEntity : BaseCharacterEntity
{
    public const string ANIM_KEY_IS_DEAD = "IsDead";
    public const string ANIM_KEY_SPEED = "Speed";
    public const string ANIM_KEY_ACTION_STATE = "ActionState";
    public const string ANIM_KEY_DO_ACTION = "DoAction";
    public const string ANIM_KEY_HURT = "Hurt";
    public const int ACTION_ATTACK = -100;

    [HideInInspector]
    public bool forcePlayMoving;
    [HideInInspector]
    public bool forceHideCharacterStats;
    [HideInInspector]
    public int currentTimeCount;
    [HideInInspector]
    public bool selectable;
    [HideInInspector]
    public UICharacterStats uiCharacterStats;

    public GamePlayFormation CastedFormation { get { return Formation as GamePlayFormation; } }
    public GamePlayManager CastedManager { get { return BaseGamePlayManager.Singleton as GamePlayManager; } }
    public override bool IsActiveCharacter { get { return CastedManager.ActiveCharacter == this; } }
    public override bool IsPlayerCharacter { get { return CastedFormation != null && CastedFormation == CastedManager.playerFormation; } }
    public int Action { get; private set; }
    public bool IsDoingAction { get; private set; }
    public bool IsMovingToTarget { get; private set; }
    public CharacterSkill SelectedSkill { get { return Skills[Action] as CharacterSkill; } }
    public CharacterEntity ActionTarget { get; private set; }
    public readonly List<Damage> Damages = new List<Damage>();
    private Vector3 targetPosition;
    private CharacterEntity targetCharacter;
    private Coroutine movingCoroutine;
    private bool isReachedTargetCharacter;

    #region Temp components
    private Rigidbody cacheRigidbody;
    public Rigidbody CacheRigidbody
    {
        get
        {
            if (cacheRigidbody == null)
                cacheRigidbody = GetComponent<Rigidbody>();
            return cacheRigidbody;
        }
    }
    private CapsuleCollider cacheCapsuleCollider;
    public CapsuleCollider CacheCapsuleCollider
    {
        get
        {
            if (cacheCapsuleCollider == null)
                cacheCapsuleCollider = GetComponent<CapsuleCollider>();
            return cacheCapsuleCollider;
        }
    }
    private TargetingRigidbody cacheTargetingRigidbody;
    public TargetingRigidbody CacheTargetingRigidbody
    {
        get
        {
            if (cacheTargetingRigidbody == null)
                cacheTargetingRigidbody = GetComponent<TargetingRigidbody>();
            return cacheTargetingRigidbody;
        }
    }
    #endregion

    #region Unity Functions
    protected override void Awake()
    {
        base.Awake();
        CacheCapsuleCollider.isTrigger = true;
    }

    private void Update()
    {
        if (Item == null)
        {
            // For show in viewers
            CacheAnimator.SetBool(ANIM_KEY_IS_DEAD, false);
            return;
        }
        CacheAnimator.SetBool(ANIM_KEY_IS_DEAD, Hp <= 0);
        if (Hp > 0)
        {
            var moveSpeed = CacheRigidbody.velocity.magnitude;
            // Assume that character is moving by set moveSpeed = 1
            if (forcePlayMoving)
                moveSpeed = 1;
            CacheAnimator.SetFloat(ANIM_KEY_SPEED, moveSpeed);
            if (uiCharacterStats != null)
            {
                if (forceHideCharacterStats)
                    uiCharacterStats.HideCharacterStats();
                else
                    uiCharacterStats.ShowCharacterStats();
            }
        }
        else
        {
            if (uiCharacterStats != null)
                uiCharacterStats.HideCharacterStats();
        }
    }

    private void OnTriggerStay(Collider other)
    {
        if (targetCharacter != null && targetCharacter == other.GetComponent<CharacterEntity>())
            isReachedTargetCharacter = true;
    }

    private void OnDestroy()
    {
        if (uiCharacterStats != null)
            Destroy(uiCharacterStats.gameObject);
    }
    #endregion

    #region Damage/Dead/Revive/Turn/Buff

    public override bool ReceiveDamage(
        BaseCharacterEntity attacker,
        float pAtkRate = 1f,
        float mAtkRate = 1f,
        int hitCount = 1,
        int fixDamage = 0)
    {
        if (base.ReceiveDamage(
            attacker,
            pAtkRate,
            mAtkRate,
            hitCount,
            fixDamage))
        {
            // Play hurt animation
            CacheAnimator.ResetTrigger(ANIM_KEY_HURT);
            CacheAnimator.SetTrigger(ANIM_KEY_HURT);
            return true;
        }
        return false;
    }

    public override void Dead()
    {
        base.Dead();
        ClearActionState();
    }

    public void DecreaseBuffsTurn()
    {
        var keys = new List<string>(Buffs.Keys);
        for (var i = keys.Count - 1; i >= 0; --i)
        {
            var key = keys[i];
            if (!Buffs.ContainsKey(key))
                continue;

            var buff = Buffs[key] as CharacterBuff;
            buff.IncreaseTurnsCount();
            if (buff.IsEnd())
            {
                buff.BuffRemove();
                Buffs.Remove(key);
            }
        }
    }

    public void DecreaseSkillsTurn()
    {
        for (var i = Skills.Count - 1; i >= 0; --i)
        {
            var skill = Skills[i] as CharacterSkill;
            skill.IncreaseTurnsCount();
        }
    }
    #endregion

    #region Movement/Actions
    public Coroutine MoveTo(Vector3 position, float speed)
    {
        if (IsMovingToTarget)
            StopCoroutine(movingCoroutine);
        IsMovingToTarget = true;
        isReachedTargetCharacter = false;
        targetPosition = position;
        movingCoroutine = StartCoroutine(MoveToRoutine(position, speed));
        return movingCoroutine;
    }

    IEnumerator MoveToRoutine(Vector3 position, float speed)
    {
        CacheTargetingRigidbody.StartMove(position, speed);
        while (true)
        {
            if (!CacheTargetingRigidbody.IsMoving || isReachedTargetCharacter)
            {
                IsMovingToTarget = false;
                CacheTargetingRigidbody.StopMove();
                if (targetCharacter == null)
                {
                    TurnToEnemyFormation();
                    CacheTransform.position = targetPosition;
                }
                targetCharacter = null;
                break;
            }
            yield return 0;
        }
    }

    public Coroutine MoveTo(CharacterEntity character, float speed)
    {
        targetCharacter = character;
        return MoveTo(character.CacheTransform.position, speed);
    }

    public void TurnToEnemyFormation()
    {
        Quaternion headingRotation;
        if (CastedFormation != null && CastedFormation.TryGetHeadingToFoeRotation(out headingRotation))
            CacheTransform.rotation = headingRotation;
    }

    public void ClearActionState()
    {
        CacheAnimator.SetInteger(ANIM_KEY_ACTION_STATE, 0);
        CacheAnimator.SetBool(ANIM_KEY_DO_ACTION, false);
    }

    public bool SetAction(int action)
    {
        if (action == ACTION_ATTACK || (action >= 0 && action < Skills.Count))
        {
            Action = action;
            CastedManager.ShowTargetScopesOrDoAction(this);
            return true;
        }
        return false;
    }

    public bool DoAction(CharacterEntity target)
    {
        if (target == null)
            return false;

        if (Action == ACTION_ATTACK)
        {
            // Cannot attack self or same team character
            if (target == this || IsSameTeamWith(target) || target.Hp <= 0)
                return false;
        }
        else
        {
            if (SelectedSkill == null || !SelectedSkill.IsReady())
                return false;

            switch (SelectedSkill.CastedSkill.usageScope)
            {
                case SkillUsageScope.Self:
                    if (target != this || target.Hp <= 0)
                        return false;
                    break;
                case SkillUsageScope.Ally:
                    if (!IsSameTeamWith(target) || target.Hp <= 0)
                        return false;
                    break;
                case SkillUsageScope.All:
                    if (target.Hp <= 0)
                        return false;
                    break;
                case SkillUsageScope.Enemy:
                    if (target == this || IsSameTeamWith(target) || target.Hp <= 0)
                        return false;
                    break;
                case SkillUsageScope.DeadAlly:
                    if (!IsSameTeamWith(target) || target.Hp > 0)
                        return false;
                    break;
            }
        }
        ActionTarget = target;
        DoAction();
        return true;
    }

    public void RandomAction()
    {
        // Random Action
        // Dictionary of actionId, weight
        Dictionary<int, int> actions = new Dictionary<int, int>();
        actions.Add(ACTION_ATTACK, 5);
        for (var i = 0; i < Skills.Count; ++i)
        {
            var skill = Skills[i] as CharacterSkill;
            if (skill == null || !skill.IsReady())
                continue;
            actions.Add(i, 5);
        }
        Action = WeightedRandomizer.From(actions).TakeOne();
        // Random Target
        if (Action == ACTION_ATTACK)
        {
            var foes = CastedManager.GetFoes(this);
            Random.InitState(System.DateTime.Now.Millisecond);
            if (foes != null && foes.Count > 0)
                ActionTarget = foes[Random.Range(0, foes.Count - 1)] as CharacterEntity;
            else
                ActionTarget = null;
        }
        else
        {
            switch (SelectedSkill.CastedSkill.usageScope)
            {
                case SkillUsageScope.Self:
                    ActionTarget = this;
                    break;
                case SkillUsageScope.Ally:
                    var allies = CastedManager.GetAllies(this);
                    Random.InitState(System.DateTime.Now.Millisecond);
                    if (allies != null && allies.Count > 0)
                        ActionTarget = allies[Random.Range(0, allies.Count)] as CharacterEntity;
                    else
                        ActionTarget = null;
                    break;
                case SkillUsageScope.Enemy:
                    var foes = CastedManager.GetFoes(this);
                    Random.InitState(System.DateTime.Now.Millisecond);
                    if (foes != null && foes.Count > 0)
                        ActionTarget = foes[Random.Range(0, foes.Count)] as CharacterEntity;
                    else
                        ActionTarget = null;
                    break;
                case SkillUsageScope.All:
                    var all = CastedManager.GetAllCharacters();
                    Random.InitState(System.DateTime.Now.Millisecond);
                    if (all != null && all.Count > 0)
                        ActionTarget = all[Random.Range(0, all.Count)] as CharacterEntity;
                    else
                        ActionTarget = null;
                    break;
                case SkillUsageScope.DeadAlly:
                    var deadAllies = CastedManager.GetAllies(this, true);
                    Random.InitState(System.DateTime.Now.Millisecond);
                    if (deadAllies != null && deadAllies.Count > 0)
                        ActionTarget = deadAllies[Random.Range(0, deadAllies.Count)] as CharacterEntity;
                    else
                        ActionTarget = null;
                    break;
            }
        }
        if (ActionTarget == null)
        {
            RandomAction();
            return;
        }
        DoAction();
    }

    private void DoAction()
    {
        if (IsDoingAction)
            return;

        if (Action == ACTION_ATTACK)
        {
            StartCoroutine(DoAttackActionRoutine());
        }
        else
        {
            SelectedSkill.OnUseSkill();
            StartCoroutine(DoSkillActionRoutine());
        }
    }

    IEnumerator DoAttackActionRoutine()
    {
        IsDoingAction = true;
        AttackAnimationData attackAnimation = null;
        if (AttackAnimations.Count > 0)
            attackAnimation = AttackAnimations[Random.Range(0, AttackAnimations.Count - 1)] as AttackAnimationData;
        if (!attackAnimation.GetIsRangeAttack())
        {
            // Move to target character
            yield return MoveTo(ActionTarget, CastedManager.doActionMoveSpeed);
        }
        // Play attack animation
        if (attackAnimation != null)
        {
            switch (attackAnimation.type)
            {
                case AnimationDataType.ChangeAnimationByState:
                    CacheAnimator.SetInteger(ANIM_KEY_ACTION_STATE, attackAnimation.GetAnimationActionState());
                    break;
                case AnimationDataType.ChangeAnimationByClip:
                    ChangeActionClip(attackAnimation.GetAnimationClip());
                    CacheAnimator.SetBool(ANIM_KEY_DO_ACTION, true);
                    break;
            }
        }
        yield return new WaitForSeconds(attackAnimation.GetHitDuration());
        // Apply damage
        Attack(ActionTarget, attackAnimation.GetDamage() as Damage);
        // Wait damages done
        while (Damages.Count > 0)
        {
            yield return 0;
        }
        // End attack
        var endAttackDuration = attackAnimation.GetAnimationDuration() - attackAnimation.GetHitDuration();
        if (endAttackDuration < 0)
            endAttackDuration = 0;
        yield return new WaitForSeconds(endAttackDuration);
        ClearActionState();
        yield return MoveTo(Container.position, CastedManager.actionDoneMoveSpeed);
        NotifyEndAction();
        IsDoingAction = false;
    }

    IEnumerator DoSkillActionRoutine()
    {
        IsDoingAction = true;
        var skill = SelectedSkill.CastedSkill;
        var skillCastAnimation = skill.castAnimation as SkillCastAnimationData;
        // Cast
        if (skillCastAnimation.GetCastAtMapCenter())
            yield return MoveTo(CastedManager.MapCenterPosition, CastedManager.doActionMoveSpeed);
        var castEffects = skillCastAnimation.GetCastEffects();
        var effects = new List<GameEffect>();
        if (castEffects != null)
            effects.AddRange(castEffects.InstantiatesTo(this));
        // Play cast animation
        if (skillCastAnimation != null)
        {
            switch (skillCastAnimation.type)
            {
                case AnimationDataType.ChangeAnimationByState:
                    CacheAnimator.SetInteger(ANIM_KEY_ACTION_STATE, skillCastAnimation.GetAnimationActionState());
                    break;
                case AnimationDataType.ChangeAnimationByClip:
                    ChangeActionClip(skillCastAnimation.GetAnimationClip());
                    CacheAnimator.SetBool(ANIM_KEY_DO_ACTION, true);
                    break;
            }
        }
        yield return new WaitForSeconds(skillCastAnimation.GetAnimationDuration());
        ClearActionState();
        foreach (var effect in effects)
        {
            effect.DestroyEffect();
        }
        effects.Clear();
        if (skill.reviveCharacter && ActionTarget.Hp <= 0)
            ActionTarget.Hp = 1;
        // Buffs
        yield return StartCoroutine(ApplyBuffsRoutine(BuffType.Buff));
        // Attacks
        yield return StartCoroutine(SkillAttackRoutine());
        // Nerfs
        yield return StartCoroutine(ApplyBuffsRoutine(BuffType.Nerf));
        // Move back to formation
        yield return MoveTo(Container.position, CastedManager.actionDoneMoveSpeed);
        NotifyEndAction();
        IsDoingAction = false;
    }

    IEnumerator ApplyBuffsRoutine(BuffType type)
    {
        var level = SelectedSkill.Level;
        var skill = SelectedSkill.CastedSkill;
        for (var i = 0; i < skill.buffs.Length; ++i)
        {
            var buff = skill.buffs[i];
            if (buff == null || buff.type != type)
                continue;

            var allies = CastedManager.GetAllies(this);
            var foes = CastedManager.GetFoes(this);
            if (buff.RandomToApply(level))
            {
                // Apply buffs to selected targets
                switch (buff.buffScope)
                {
                    case BuffScope.SelectedTarget:
                    case BuffScope.SelectedAndOneRandomTargets:
                    case BuffScope.SelectedAndTwoRandomTargets:
                    case BuffScope.SelectedAndThreeRandomTargets:
                        ActionTarget.ApplyBuff(this, level, skill, i);
                        break;
                }

                int randomAllyCount = 0;
                int randomFoeCount = 0;
                // Buff scope
                switch (buff.buffScope)
                {
                    case BuffScope.Self:
                        ApplyBuff(this, level, skill, i);
                        continue;
                    case BuffScope.SelectedAndOneRandomTargets:
                        if (ActionTarget.IsSameTeamWith(this))
                            randomAllyCount = 1;
                        else if (!ActionTarget.IsSameTeamWith(this))
                            randomFoeCount = 1;
                        break;
                    case BuffScope.SelectedAndTwoRandomTargets:
                        if (ActionTarget.IsSameTeamWith(this))
                            randomAllyCount = 2;
                        else if (!ActionTarget.IsSameTeamWith(this))
                            randomFoeCount = 2;
                        break;
                    case BuffScope.SelectedAndThreeRandomTargets:
                        if (ActionTarget.IsSameTeamWith(this))
                            randomAllyCount = 3;
                        else if (!ActionTarget.IsSameTeamWith(this))
                            randomFoeCount = 3;
                        break;
                    case BuffScope.OneRandomAlly:
                        randomAllyCount = 1;
                        break;
                    case BuffScope.TwoRandomAllies:
                        randomAllyCount = 2;
                        break;
                    case BuffScope.ThreeRandomAllies:
                        randomAllyCount = 3;
                        break;
                    case BuffScope.FourRandomAllies:
                        randomAllyCount = 4;
                        break;
                    case BuffScope.AllAllies:
                        randomAllyCount = allies.Count;
                        break;
                    case BuffScope.OneRandomEnemy:
                        randomFoeCount = 1;
                        break;
                    case BuffScope.TwoRandomEnemies:
                        randomFoeCount = 2;
                        break;
                    case BuffScope.ThreeRandomEnemies:
                        randomFoeCount = 3;
                        break;
                    case BuffScope.FourRandomEnemies:
                        randomFoeCount = 4;
                        break;
                    case BuffScope.AllEnemies:
                        randomFoeCount = foes.Count;
                        break;
                    case BuffScope.All:
                        randomAllyCount = allies.Count;
                        randomFoeCount = foes.Count;
                        break;
                }
                // End buff scope
                // Don't apply buffs to character that already applied
                if (randomAllyCount > 0)
                {
                    allies.Remove(ActionTarget);
                    while (allies.Count > 0 && randomAllyCount > 0)
                    {
                        Random.InitState(System.DateTime.Now.Millisecond);
                        var randomIndex = Random.Range(0, allies.Count - 1);
                        var applyBuffTarget = allies[randomIndex];
                        applyBuffTarget.ApplyBuff(this, level, skill, i);
                        allies.RemoveAt(randomIndex);
                        --randomAllyCount;
                    }
                }
                // Don't apply buffs to character that already applied
                if (randomFoeCount > 0)
                {
                    foes.Remove(ActionTarget);
                    while (foes.Count > 0 && randomFoeCount > 0)
                    {
                        Random.InitState(System.DateTime.Now.Millisecond);
                        var randomIndex = Random.Range(0, foes.Count - 1);
                        var applyBuffTarget = foes[randomIndex];
                        applyBuffTarget.ApplyBuff(this, level, skill, i);
                        foes.RemoveAt(randomIndex);
                        --randomFoeCount;
                    }
                }
            }
        }
        yield return 0;
    }

    IEnumerator SkillAttackRoutine()
    {
        var level = SelectedSkill.Level;
        var skill = SelectedSkill.CastedSkill;
        if (skill.attacks.Length > 0)
        {
            var isAlreadyReachedTarget = false;
            foreach (var attack in skill.attacks)
            {
                var foes = CastedManager.GetFoes(this);
                var attackDamage = attack.attackDamage;
                var attackAnimation = attack.attackAnimation as AttackAnimationData;
                if (!attackAnimation.GetIsRangeAttack() && !isAlreadyReachedTarget)
                {
                    // Move to target character
                    yield return MoveTo(ActionTarget, CastedManager.doActionMoveSpeed);
                    isAlreadyReachedTarget = true;
                }
                // Play attack animation
                if (attackAnimation != null)
                {
                    switch (attackAnimation.type)
                    {
                        case AnimationDataType.ChangeAnimationByState:
                            CacheAnimator.SetInteger(ANIM_KEY_ACTION_STATE, attackAnimation.GetAnimationActionState());
                            break;
                        case AnimationDataType.ChangeAnimationByClip:
                            ChangeActionClip(attackAnimation.GetAnimationClip());
                            CacheAnimator.SetBool(ANIM_KEY_DO_ACTION, true);
                            break;
                    }
                }
                yield return new WaitForSeconds(attackAnimation.GetHitDuration());
                // Apply damage
                // Attack to selected target
                switch (attack.attackScope)
                {
                    case AttackScope.SelectedTarget:
                    case AttackScope.SelectedAndOneRandomTargets:
                    case AttackScope.SelectedAndTwoRandomTargets:
                    case AttackScope.SelectedAndThreeRandomTargets:
                        Attack(ActionTarget, attackAnimation.GetDamage() as Damage, attackDamage.GetPAtkDamageRate(level), attackDamage.GetMAtkDamageRate(level), attackDamage.hitCount, (int)attackDamage.GetFixDamage(level));
                        break;
                }
                // Attack to random targets
                int randomFoeCount = 0;
                // Attack scope
                switch (attack.attackScope)
                {
                    case AttackScope.SelectedAndOneRandomTargets:
                    case AttackScope.OneRandomEnemy:
                        randomFoeCount = 1;
                        break;
                    case AttackScope.SelectedAndTwoRandomTargets:
                    case AttackScope.TwoRandomEnemies:
                        randomFoeCount = 2;
                        break;
                    case AttackScope.SelectedAndThreeRandomTargets:
                    case AttackScope.ThreeRandomEnemies:
                        randomFoeCount = 3;
                        break;
                    case AttackScope.FourRandomEnemies:
                        randomFoeCount = 4;
                        break;
                    case AttackScope.AllEnemies:
                        randomFoeCount = foes.Count;
                        break;
                }
                // End attack scope
                while (foes.Count > 0 && randomFoeCount > 0)
                {
                    Random.InitState(System.DateTime.Now.Millisecond);
                    var randomIndex = Random.Range(0, foes.Count - 1);
                    var attackingTarget = foes[randomIndex] as CharacterEntity;
                    Attack(attackingTarget, attackAnimation.GetDamage() as Damage, attackDamage.GetPAtkDamageRate(level), attackDamage.GetMAtkDamageRate(level), attackDamage.hitCount, (int)attackDamage.GetFixDamage(level));
                    foes.RemoveAt(randomIndex);
                    --randomFoeCount;
                }
                // End attack
                var endAttackDuration = attackAnimation.GetAnimationDuration() - attackAnimation.GetHitDuration();
                if (endAttackDuration < 0)
                    endAttackDuration = 0;
                yield return new WaitForSeconds(endAttackDuration);
                ClearActionState();
                yield return 0;
            }
            // End attack loop
            // Wait damages done
            while (Damages.Count > 0)
            {
                yield return 0;
            }
        }
    }

    public void ResetStates()
    {
        Action = ACTION_ATTACK;
        ActionTarget = null;
        IsDoingAction = false;
    }

    public void NotifyEndAction()
    {
        CastedManager.NotifyEndAction(this);
    }
    #endregion

    #region Misc
    public bool IsSameTeamWith(CharacterEntity target)
    {
        return target != null && Formation == target.Formation;
    }

    public override void SetFormation(BaseGamePlayFormation formation, int position, Transform container)
    {
        if (container == null)
            return;

        base.SetFormation(formation, position, container);

        Quaternion headingRotation;
        if (CastedFormation != null && CastedFormation.TryGetHeadingToFoeRotation(out headingRotation))
        {
            CacheTransform.rotation = headingRotation;
            if (CastedManager != null)
                CacheTransform.position -= CastedManager.spawnOffset * CacheTransform.forward;
        }
    }

    public override BaseCharacterSkill NewSkill(int level, BaseSkill skill)
    {
        return new CharacterSkill(level, skill);
    }

    public override BaseCharacterBuff NewBuff(int level, BaseSkill skill, int buffIndex, BaseCharacterEntity giver, BaseCharacterEntity receiver)
    {
        return new CharacterBuff(level, skill, buffIndex, giver, receiver);
    }
    #endregion
}
