using EntityComponent;

namespace EntityVisualEffect
{
	public enum DeathEffectType
	{
        DissolveDeath,
    }

    // 死亡效果具有唯一性
    // 以生效早的为准，后来者不可覆盖替换当前效果
	public abstract class DeathEffect : BaseEffect
	{
        public DeathEffectType SubType { get; set; }
        protected DeathEffect() {}
	}
}
