import { AuthorRole } from '../../models/author-role.model';
export interface StreamingChatMessageContent {
  choiceIndex: number;
  modelId?: string | null;
  metadata?: any | null;
  content?: string | null;
  authorName?: string | null;
  role?: AuthorRole | null;

}
