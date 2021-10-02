import { ApolloClient, InMemoryCache } from "@apollo/client/core";

function replaceWithIncoming(existing, incoming) {
  return incoming;
}

const client = new ApolloClient({
  uri: "/graphql",
  // defaultOptions: {
  //   watchQuery: {
  //     fetchPolicy: "cache-and-network"
  //   }
  // },
  cache: new InMemoryCache({
    typePolicies: {
      GroupGraphType: {
        fields: {
          invites: {
            merge: replaceWithIncoming
          }
        }
      },
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
      UpcomingSessionGraphType: {
        fields: {
          participants: {
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
