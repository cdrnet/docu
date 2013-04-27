using Docu.Documentation;
using Docu.Events;
using Docu.Parsing.Model;
using Example;
using NUnit.Framework;

namespace Docu.Tests.Documentation.DocumentModelGeneratorTests
{
    [TestFixture]
    public class Events : BaseDocumentModelGeneratorFixture
    {
        [Test]
        public void ShouldHaveEventsInTypes()
        {
            var model = new DocumentModel(StubParser, new EventAggregator());
            var members = new IDocumentationMember[]
                {
                    Type<Second>(@"<member name=""T:Example.Second"" />"),
                    Event<Second>(@"<member name=""E:Example.Second.AnEvent"" />", "AnEvent"),
                };
            var namespaces = model.Create(members);
            var ev = typeof (Second).GetEvent("AnEvent");

            namespaces[0].Types[0].Events.ShouldContain(x => x.IsIdentifiedBy(Identifier.FromEvent(ev, typeof (Second))));
        }
    }
}
