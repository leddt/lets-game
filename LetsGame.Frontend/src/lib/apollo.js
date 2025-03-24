import {
  ApolloClient,
  HttpLink,
  InMemoryCache,
  split,
} from "@apollo/client/core";
import { SubscriptionClient } from "subscriptions-transport-ws";
import { WebSocketLink } from "@apollo/client/link/ws";
import { getMainDefinition } from "@apollo/client/utilities";

function replaceWithIncoming(existing, incoming) {
  return incoming;
}

const httpLink = new HttpLink({
  uri: "/graphql",
});

const protocol = window.location.protocol === "https:" ? "wss" : "ws";
let subscriptionClient;

function createLink() {
  subscriptionClient?.close();
  subscriptionClient = new SubscriptionClient(
    `${protocol}://${window.location.host}/graphql`,
    { reconnect: true }
  );

  const wsLink = new WebSocketLink(subscriptionClient);

  return split(
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
}

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
  cache,
});

if (document.visibilityState === "visible") {
  client.setLink(createLink());
}

document.addEventListener("visibilitychange", () => {
  if (document.visibilityState === "hidden") {
    subscriptionClient?.close();
    subscriptionClient = null;
  } else {
    if (!subscriptionClient) {
      window.location.reload();
    }
  }
});

export default client;
