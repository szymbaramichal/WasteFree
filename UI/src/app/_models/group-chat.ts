export interface GroupChatMessage {
  id: string;
  groupId: string;
  userId: string;
  username: string;
  avatarName?: string | null;
  content: string;
  sentAtUtc: string;
}

export type GroupChatConnectionState = 'disconnected' | 'connecting' | 'connected' | 'reconnecting' | 'error';
