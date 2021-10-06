import { ApolloClient, InMemoryCache } from "@apollo/client/core/index.js";

function replaceWithIncoming(existing, incoming) {
  return incoming;
}

const client = new ApolloClient({
  uri: "/graphql",
  cache: new InMemoryCache({
    typePolicies: {
      GroupGraphType: {
        fields: {
          invites: {
            merge: replaceWithIncoming,
          },
          games: {
            merge: replaceWithIncoming,
          },
        },
      },
      ProposedSessionGraphType: {
        fields: {
          missingVotes: {
            merge: replaceWithIncoming,
          },
          cantPlays: {
            merge: replaceWithIncoming,
          },
        },
      },
      UpcomingSessionGraphType: {
        fields: {
          participants: {
            merge: replaceWithIncoming,
          },
        },
      },
      SessionSlotGraphType: {
        fields: {
          voters: {
            merge: replaceWithIncoming,
          },
        },
      },
    },
  }),
});

export default client;
