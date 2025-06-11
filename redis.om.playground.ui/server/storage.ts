import { users, persons, type User, type InsertUser, type Person, type InsertPerson } from "@shared/schema";

export interface IStorage {
  getUser(id: number): Promise<User | undefined>;
  getUserByUsername(username: string): Promise<User | undefined>;
  createUser(user: InsertUser): Promise<User>;
  
  // Person management methods
  getPerson(id: number): Promise<Person | undefined>;
  getPersons(limit: number, offset: number): Promise<Person[]>;
  getPersonsCount(): Promise<number>;
  searchPersons(query: string, limit: number, offset: number): Promise<Person[]>;
  searchPersonsCount(query: string): Promise<number>;
  createPerson(person: InsertPerson): Promise<Person>;
  updatePerson(id: number, person: Partial<InsertPerson>): Promise<Person | undefined>;
  deletePerson(id: number): Promise<boolean>;
}

export class MemStorage implements IStorage {
  private users: Map<number, User>;
  private persons: Map<number, Person>;
  private currentUserId: number;
  private currentPersonId: number;

  constructor() {
    this.users = new Map();
    this.persons = new Map();
    this.currentUserId = 1;
    this.currentPersonId = 1;
  }

  async getUser(id: number): Promise<User | undefined> {
    return this.users.get(id);
  }

  async getUserByUsername(username: string): Promise<User | undefined> {
    return Array.from(this.users.values()).find(
      (user) => user.username === username,
    );
  }

  async createUser(insertUser: InsertUser): Promise<User> {
    const id = this.currentUserId++;
    const user: User = { ...insertUser, id };
    this.users.set(id, user);
    return user;
  }

  async getPerson(id: number): Promise<Person | undefined> {
    return this.persons.get(id);
  }

  async getPersons(limit: number, offset: number): Promise<Person[]> {
    const allPersons = Array.from(this.persons.values());
    return allPersons.slice(offset, offset + limit);
  }

  async getPersonsCount(): Promise<number> {
    return this.persons.size;
  }

  async searchPersons(query: string, limit: number, offset: number): Promise<Person[]> {
    const allPersons = Array.from(this.persons.values());
    const filteredPersons = allPersons.filter(person => 
      person.firstName.toLowerCase().includes(query.toLowerCase()) ||
      person.lastName.toLowerCase().includes(query.toLowerCase()) ||
      person.personalStatement.toLowerCase().includes(query.toLowerCase())
    );
    return filteredPersons.slice(offset, offset + limit);
  }

  async searchPersonsCount(query: string): Promise<number> {
    const allPersons = Array.from(this.persons.values());
    return allPersons.filter(person => 
      person.firstName.toLowerCase().includes(query.toLowerCase()) ||
      person.lastName.toLowerCase().includes(query.toLowerCase()) ||
      person.personalStatement.toLowerCase().includes(query.toLowerCase())
    ).length;
  }

  async createPerson(insertPerson: InsertPerson): Promise<Person> {
    const id = this.currentPersonId++;
    const person: Person = { ...insertPerson, id };
    this.persons.set(id, person);
    return person;
  }

  async updatePerson(id: number, updateData: Partial<InsertPerson>): Promise<Person | undefined> {
    const existingPerson = this.persons.get(id);
    if (!existingPerson) return undefined;
    
    const updatedPerson: Person = { ...existingPerson, ...updateData };
    this.persons.set(id, updatedPerson);
    return updatedPerson;
  }

  async deletePerson(id: number): Promise<boolean> {
    return this.persons.delete(id);
  }
}

export const storage = new MemStorage();
