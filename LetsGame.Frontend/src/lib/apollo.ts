import { ApolloClient, InMemoryCache } from "@apollo/client/core";

function replaceWithIncoming(existing, incoming) {
  return incoming;
}

const client = new ApolloClient({
  uri: "/graphql",
  cache: new InMemoryCache({
    typePolicies: {
      ProposedSessionGraphType: {
        fields: {
          missingVotes: {
            merge: replaceWithIncoming
          },
          cantPlays: {
            merge: replaceWithIncoming
          }
        }
      },
      SessionSlotGraphType: {
        fields: {
          voters: {
            merge: replaceWithIncoming
          }
        }
      }
    }
  })
});

export default client;
