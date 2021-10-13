import {
  addDays,
  endOfDay,
  format,
  formatRelative,
  isAfter,
  isBefore,
  startOfDay,
} from "date-fns";

import { capitalize } from "./string-helpers";

export function friendlyDateTime(date, capitalizeResult = true) {
  if (!date) return "";
  if (typeof date === "string") date = new Date(date);

  const now = new Date();
  const today = startOfDay(now);
  const tomorrow = endOfDay(addDays(now, 1));

  if (isBefore(date, tomorrow) && isAfter(date, today)) {
    const result = formatRelative(date, now);
    return capitalizeResult ? capitalize(result) : result;
  } else {
    return format(date, "EEEE, MMM do 'at' h:mm a");
  }
}

export function time(date) {
  if (!date) return "";
  if (typeof date === "string") date = new Date(date);

  return format(date, "h:mm a");
}
