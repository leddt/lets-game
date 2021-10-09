import {
  ApolloClient,
  HttpLink,
  InMemoryCache,
  split,
} from "@apollo/client/core";
import { WebSocketLink } from "@apollo/client/link/ws";
import { getMainDefinition } from "@apollo/client/utilities";

function replaceWithIncoming(existing, incoming) {
  return incoming;
}

const httpLink = new HttpLink({
  uri: "/graphql",
});

const protocol = window.location.protocol === "https:" ? "wss" : "ws";
const wsLink = new WebSocketLink({
  uri: `${protocol}://${window.location.host}/graphql`,
  options: {
    reconnect: true,
  },
});

const splitLink = split(
  ({ query }) => {
    const definition = getMainDefinition(query);
    return (
      definition.kind === "OperationDefinition" &&
      definition.operation === "subscription"
    );
  },
  wsLink,
  httpLink
);

export const cache = new InMemoryCache({
  typePolicies: {
    GroupGraphType: {
      fields: {
        invites: {
          merge: replaceWithIncoming,
        },
        games: {
          merge: replaceWithIncoming,
        },
        proposedSessions: {
          merge: replaceWithIncoming,
        },
        upcomingSessions: {
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
});

const client = new ApolloClient({
  link: splitLink,
  cache,
});

export default client;
