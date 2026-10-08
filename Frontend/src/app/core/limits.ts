// Mirrors Backend/src/QandA.Api/Domain/Limits.cs, used for client-side validation and error texts.
export const LIMITS = {
  emailMax: 254,
  displayNameMin: 2,
  displayNameMax: 50,
  passwordMin: 8,
  passwordMax: 128,
  titleMax: 200,
  descriptionMax: 1000,
  optionTextMax: 200,
  optionsMin: 2,
  optionsMax: 30,
  maxVotesPerUserCap: 30,
  maxOptionsPerParticipantCap: 10,
} as const;

/** Same loose check as the API: the confirmation email is the real validation. */
export const EMAIL_PATTERN = /^[^@\s]+@[^@\s]+\.[^@\s.]+$/;
