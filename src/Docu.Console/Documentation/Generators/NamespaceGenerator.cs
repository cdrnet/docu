using System;
using System.Collections.Generic;
using Docu.Parsing.Model;

namespace Docu.Documentation.Generators
{
    internal class NamespaceGenerator : IGenerator<IDocumentationMember>
    {
        readonly IDictionary<Identifier, IReferencable> matchedAssociations;

        public NamespaceGenerator(IDictionary<Identifier, IReferencable> matchedAssociations)
        {
            this.matchedAssociations = matchedAssociations;
        }

        public void Add(List<Namespace> namespaces, IDocumentationMember association)
        {
            if (association.TargetType.Namespace == null)
            {
                throw new NullReferenceException(
                    string.Format("There was no namespace found for {0}", association.TargetType.AssemblyQualifiedName));
            }

            NamespaceIdentifier ns = Identifier.FromNamespace(association.TargetType.Namespace);

            if (!namespaces.Exists(x => x.IsIdentifiedBy(ns)))
            {
                Namespace doc = Namespace.Unresolved(ns);
                matchedAssociations.Add(association.Name.CloneAsNamespace(), doc);
                namespaces.Add(doc);
            }
        }
    }
}
