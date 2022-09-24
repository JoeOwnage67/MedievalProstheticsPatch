using System.Collections.Generic;
using RimWorld;
using Verse;

namespace MM
{
    // Token: 0x0200001B RID: 27
    public class HediffComp_MMRegen : HediffComp
    {
        // Token: 0x04000048 RID: 72
        public const string AYScar = "AYScarCreamHigh";

        // Token: 0x04000049 RID: 73
        private int ticksToHeal;

        // Token: 0x17000005 RID: 5
        // (get) Token: 0x0600004F RID: 79 RVA: 0x0000401E File Offset: 0x0000221E
        public HediffCompProperties_MMRegen MMProps => (HediffCompProperties_MMRegen) props;

        // Token: 0x06000050 RID: 80 RVA: 0x0000402B File Offset: 0x0000222B
        public override void CompPostMake()
        {
            base.CompPostMake();
            ResetTicksToHeal();
        }

        // Token: 0x06000051 RID: 81 RVA: 0x0000403C File Offset: 0x0000223C
        public void ResetTicksToHeal()
        {
            var period = 2500;
            if (MMProps.RegenHoursMin > 0 && MMProps.RegenHoursMax > 0 &&
                MMProps.RegenHoursMax >= MMProps.RegenHoursMin)
            {
                ticksToHeal = Rand.Range(MMProps.RegenHoursMin, MMProps.RegenHoursMax) * period;
                return;
            }

            ticksToHeal = Rand.Range(12, 24) * period;
        }

        // Token: 0x06000052 RID: 82 RVA: 0x000040B8 File Offset: 0x000022B8
        public override void CompPostTick(ref float severityAdjustment)
        {
            ticksToHeal--;
            if (ticksToHeal > 0)
            {
                return;
            }

            TryHealRandomOldWound();
            ResetTicksToHeal();
        }

        // Token: 0x06000053 RID: 83 RVA: 0x000040E4 File Offset: 0x000022E4
        public void TryHealRandomOldWound()
        {
            var healAmount = MMProps.RegenHealVal;
            var candidates = new List<Hediff>();
            var pawn = Pawn;
            List<Hediff> list;
            if (pawn == null)
            {
                list = null;
            }
            else
            {
                var health = pawn.health;
                list = health?.hediffSet.hediffs;
            }

            var hediffs = list;
            if (hediffs != null && hediffs.Count > 0)
            {
                foreach (var hediff in hediffs)
                {
                    if (hediff.def == HediffDefOf.Burn)
                        {
                            candidates.Add(hediff);
                        }
                    }
                    else if (Def.defName == "AYScarCreamHigh")
                    {
                        if (hediff.IsPermanent() || hediff.def == HediffDefOf.Burn)
                        {
                            candidates.Add(hediff);
                        }
                    }
                    else if (hediff.IsPermanent())
                    {
                        candidates.Add(hediff);
                    }
                }
            }

            if (candidates.Count <= 0)
            {
                return;
            }

            candidates.TryRandomElement(out var hediffToHeal);
            if (hediffToHeal == null)
            {
                return;
            }

            if (hediffToHeal.IsTended())
            {
                healAmount = (int) (healAmount * 1.2f);
                var healfactor = GetHealFactor(Def, hediffToHeal);
                if (healfactor > 0f)
                {
                    healAmount = (int) (healAmount * healfactor);
                    if (healAmount < 1)
                    {
                        healAmount = 1;
                    }
                }
            }

            if (hediffToHeal.Severity - healAmount <= 0f && PawnUtility.ShouldSendNotificationAbout(Pawn))
            {
                Messages.Message(
                    "MM.WoundHealed".Translate(parent.LabelCap, Pawn.LabelShort, hediffToHeal.Label,
                        Pawn.Named("PAWN")), Pawn, MessageTypeDefOf.PositiveEvent);
            }

            if (hediffToHeal.Severity - healAmount > 0f)
            {
                hediffToHeal.Severity -= healAmount;
                return;
            }

            hediffToHeal.Severity = 0f;
        }

        // Token: 0x06000054 RID: 84 RVA: 0x000042D0 File Offset: 0x000024D0
        internal float GetHealFactor(HediffDef rootDef, Hediff h)
        {
            var hf = 1f;
            if (h.def == HediffDefOf.Scratch)
            {
                hf = rootDef.defName == "MMScarCreamHigh" ? 1.5f : 1.2f;
            }
            else if (h.def == HediffDefOf.Bruise)
            {
                hf = 1.5f;
            }
            else if (h.def == HediffDefOf.Burn)
            }
            else if (h.def == DefDatabase<HediffDef>.GetNamed("Crack"))
            {
                hf = 0.8f;
            }
            else if (h.def == DefDatabase<HediffDef>.GetNamed("Crush"))
            {
                hf = 0.8f;
            }
            else if (h.def == DefDatabase<HediffDef>.GetNamed("Frostbite"))
            {
                hf = rootDef.defName == "MMScarCreamHigh" ? 1f : 0.8f;
            }

            return hf;
        }

        // Token: 0x06000055 RID: 85 RVA: 0x000043CC File Offset: 0x000025CC
        internal bool MMIsRegenInjury(Hediff h)
        {
            return h.Bleeding || h.def == HediffDefOf.Cut || h.def == HediffDefOf.Burn ||
                   h.def == HediffDefOf.Gunshot || h.def == HediffDefOf.Scratch || h.def == HediffDefOf.Stab ||
                   h.def == HediffDefOf.Bruise || h.def == HediffDefOf.Bite || h.def == HediffDefOf.Shredded ||
                   h.IsPermanent() || h.def == DefDatabase<HediffDef>.GetNamed("Crack") ||
                   h.def == DefDatabase<HediffDef>.GetNamed("Crush") ||
                   h.def == DefDatabase<HediffDef>.GetNamed("Frostbite");
        }

        // Token: 0x06000056 RID: 86 RVA: 0x00004499 File Offset: 0x00002699
        public override void CompExposeData()
        {
            Scribe_Values.Look(ref ticksToHeal, "ticksToHeal");
        }

        // Token: 0x06000057 RID: 87 RVA: 0x000044AD File Offset: 0x000026AD
        public override string CompDebugString()
        {
            return "ticksToHeal: " + ticksToHeal;
        }
    }
}
