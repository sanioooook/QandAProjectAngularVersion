// Mirrors the DTOs of Backend/src/QandA.Api (camelCase JSON, enums as camelCase strings).

/** The signed-in user's own account. */
export interface Account {
  id: number;
  email: string;
  displayName: string;
  emailConfirmed: boolean;
  /** Language of the emails; kept in sync with the UI language. */
  locale: string;
  avatarUrl: string | null;
}

/** How other users are shown (survey author, voter); never contains the email. */
export interface PublicUser {
  id: number;
  name: string;
  avatarUrl: string | null;
}

export interface AuthConfig {
  emailEnabled: boolean;
  confirmationRequired: boolean;
}

export type SurveyStatus = 'draft' | 'active' | 'closed';
export type SurveyScope = 'active' | 'mine' | 'voted';

export interface Paged<T> {
  items: T[];
  total: number;
  page: number;
  pageSize: number;
}

export interface SurveySummary {
  id: string;
  title: string;
  author: PublicUser;
  createdAt: string;
  publishedAt: string | null;
  deadline: string | null;
  status: SurveyStatus;
  optionCount: number;
  voterCount: number;
  hasVoted: boolean;
}

export interface Voter {
  userId: number;
  name: string;
  avatarUrl: string | null;
  votedAt: string;
}

export interface SurveyOption {
  id: number;
  text: string;
  votes: number;
  addedBy: string | null;
  /** Only present for the survey author. */
  voters: Voter[] | null;
}

export interface SurveyDetails {
  id: string;
  title: string;
  description: string | null;
  author: PublicUser;
  createdAt: string;
  publishedAt: string | null;
  deadline: string | null;
  status: SurveyStatus;
  maxVotesPerUser: number;
  allowParticipantOptions: boolean;
  maxOptionsPerParticipant: number;
  isAuthor: boolean;
  voterCount: number;
  totalVotes: number;
  myVotes: number[];
  myAddedOptions: number;
  canVote: boolean;
  canAddOption: boolean;
  options: SurveyOption[];
}

export interface SurveyInput {
  title: string;
  description: string | null;
  options: string[];
  deadline: string | null;
  maxVotesPerUser: number;
  allowParticipantOptions: boolean;
  maxOptionsPerParticipant: number;
  publish: boolean;
}

export interface Credentials {
  email: string;
  password: string;
}

export interface Registration extends Credentials {
  displayName: string;
  locale: string;
}
