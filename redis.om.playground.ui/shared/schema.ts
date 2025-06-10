import { z } from "zod";

export const insertUserSchema = z.object({
  username: z.string(),
  password: z.string(),
});

export const userSchema = z.object({
  id: z.number(),
  username: z.string(),
  password: z.string(),
});

export const insertPersonSchema = z.object({
  firstName: z.string(),
  lastName: z.string(),
  personalStatement: z.string(),
});

export const personSchema = z.object({
  id: z.number(),
  firstName: z.string(),
  lastName: z.string(),
  personalStatement: z.string(),
});

export type InsertUser = z.infer<typeof insertUserSchema>;
export type User = z.infer<typeof userSchema>;
export type InsertPerson = z.infer<typeof insertPersonSchema>;
export type Person = z.infer<typeof personSchema>;