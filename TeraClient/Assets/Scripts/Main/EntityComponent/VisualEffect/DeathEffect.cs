using EntityComponent;

namespace EntityVisualEffect
{
	public enum DeathEffectType
	{
        DissolveDeath,
    }

    // ����Ч������Ψһ��
    // ����Ч���Ϊ׼�������߲��ɸ����滻��ǰЧ��
	public abstract class DeathEffect : BaseEffect
	{
        public DeathEffectType SubType { get; set; }
        protected DeathEffect() {}
	}
}
