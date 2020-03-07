using System.Collections;

namespace GameLib
{
    public sealed class StartNode : FlowNode
    {
        public override string NodeName
        {
            get { return "StartNode"; }
        }

        public override void OnDrawProperty()
        {
            base.OnDrawProperty();
        }

        public override void OnDrawNode()
        {
            base.OnDrawNode();
        }

        public override IEnumerator OnExecute()
        {
            yield return base.OnExecute();

            FinishExecute();
        }
    }
}