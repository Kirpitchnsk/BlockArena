import { Rest } from './rest'

export const leaderBoardRestService = {
    get: () => Rest.get('/api/player-scores'),
    postScore: ({ username, score }) => Rest.post({ url: '/api/player-scores', data: { username, score } })
}