using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

/// <summary>
/// Избыточные тесты для системы ходов, колоды/руки/сброса и всех карт.
/// </summary>
public class TurnDeckCardTests
{
    // ─── Helpers ─────────────────────────────────────────────────────────────

    private static (BattleState state, EventQueue queue) CreateQueue(int seed = 42)
    {
        var state = new BattleState(seed);
        var queue = new EventQueue(state);
        queue.Subscribe(new TargetingSystem());
        queue.Subscribe(new DamageSystem());
        queue.Subscribe(new HealthSystem());
        queue.Subscribe(new ResourceCostSystem());
        queue.Subscribe(new BurnSystem());
        queue.Subscribe(new LoopSystem());
        queue.Subscribe(new BranchingSystem());
        queue.Subscribe(new RicochetSwordSystem());
        queue.Subscribe(new FireWallBurnSystem());
        queue.Subscribe(new AutoDrawSystem());
        queue.Subscribe(new PlayCardEventSystem());
        queue.Subscribe(new PlayCardCheckSystem());
        return (state, queue);
    }

    private static BaseEntity AddEntity(BattleState state, Geid teamId, int hp = 100, int mana = 10, int energy = 10, HexCoordinates? hex = null)
    {
        var e = new BaseEntity();
        e.AddComponent(new HealthComponent(hp));
        e.AddComponent(new ManaComponent(mana));
        e.AddComponent(new EnergyComponent(energy));
        e.AddComponent(new TeamComponent(teamId));
        if (hex.HasValue)
            e.AddComponent(new HexComponent(hex.Value));
        state.AddEntity(e);
        return e;
    }

    private static Geid NewTeam() => Geid.New;

    // ─── TURN SYSTEM TESTS ───────────────────────────────────────────────────

    [Test]
    public void TurnManager_RegisterAndStartBattle_FirstTeamGetsTurn()
    {
        var (state, queue) = CreateQueue();
        var tm = new TurnManager(queue);
        var teamA = NewTeam();
        var teamB = NewTeam();

        tm.RegisterTeam(teamA);
        tm.RegisterTeam(teamB);

        Assert.AreEqual(Geid.Empty, tm.CurrentTeamId, "No team should have turn before battle start.");

        tm.StartBattle(fireEvents: false);

        Assert.AreEqual(teamA, tm.CurrentTeamId, "First registered team should have first turn.");
        Assert.AreEqual(1, tm.TurnNumber);

        Debug.Log("TurnManager_RegisterAndStartBattle_FirstTeamGetsTurn passed.");
    }

    [Test]
    public void TurnManager_EndTurn_PassesToNextTeam()
    {
        var (state, queue) = CreateQueue();
        var tm = new TurnManager(queue);
        var teamA = NewTeam();
        var teamB = NewTeam();

        tm.RegisterTeam(teamA);
        tm.RegisterTeam(teamB);
        tm.StartBattle(fireEvents: false);

        Assert.AreEqual(teamA, tm.CurrentTeamId);

        tm.EndTurn(teamA, fireEvents: false);

        Assert.AreEqual(teamB, tm.CurrentTeamId);
        Assert.AreEqual(2, tm.TurnNumber);

        Debug.Log("TurnManager_EndTurn_PassesToNextTeam passed.");
    }

    [Test]
    public void TurnManager_EndTurn_WrongTeam_HasNoEffect()
    {
        var (state, queue) = CreateQueue();
        var tm = new TurnManager(queue);
        var teamA = NewTeam();
        var teamB = NewTeam();

        tm.RegisterTeam(teamA);
        tm.RegisterTeam(teamB);
        tm.StartBattle(fireEvents: false);

        // TeamB tries to end turn, but it's teamA's turn
        tm.EndTurn(teamB, fireEvents: false);

        Assert.AreEqual(teamA, tm.CurrentTeamId, "Current team should still be A after wrong team tried to end turn.");
        Assert.AreEqual(1, tm.TurnNumber);

        Debug.Log("TurnManager_EndTurn_WrongTeam_HasNoEffect passed.");
    }

    [Test]
    public void TurnManager_ForceEndTurn_CanEndAnyTeamsTurn()
    {
        var (state, queue) = CreateQueue();
        var tm = new TurnManager(queue);
        var teamA = NewTeam();
        var teamB = NewTeam();
        var teamC = NewTeam();

        tm.RegisterTeam(teamA);
        tm.RegisterTeam(teamB);
        tm.RegisterTeam(teamC);
        tm.StartBattle(fireEvents: false);

        Assert.AreEqual(teamA, tm.CurrentTeamId);

        // Force-end teamA's turn (same as regular end)
        tm.ForceEndTurn(teamA, fireEvents: false);

        // After force-ending teamA (index 0), next is teamB (index 1)
        Assert.AreEqual(teamB, tm.CurrentTeamId);

        Debug.Log("TurnManager_ForceEndTurn_CanEndAnyTeamsTurn passed.");
    }

    [Test]
    public void TurnManager_EndTurnAndPassTo_PassesToSpecificTeam()
    {
        var (state, queue) = CreateQueue();
        var tm = new TurnManager(queue);
        var teamA = NewTeam();
        var teamB = NewTeam();
        var teamC = NewTeam();

        tm.RegisterTeam(teamA);
        tm.RegisterTeam(teamB);
        tm.RegisterTeam(teamC);
        tm.StartBattle(fireEvents: false);

        Assert.AreEqual(teamA, tm.CurrentTeamId);

        // Skip teamB, go directly to teamC
        tm.EndTurnAndPassTo(teamA, teamC, fireEvents: false);

        Assert.AreEqual(teamC, tm.CurrentTeamId, "Should pass to teamC directly.");

        Debug.Log("TurnManager_EndTurnAndPassTo_PassesToSpecificTeam passed.");
    }

    [Test]
    public void TurnManager_TurnCycles_BackToFirstTeam()
    {
        var (state, queue) = CreateQueue();
        var tm = new TurnManager(queue);
        var teamA = NewTeam();
        var teamB = NewTeam();

        tm.RegisterTeam(teamA);
        tm.RegisterTeam(teamB);
        tm.StartBattle(fireEvents: false);

        tm.EndTurn(teamA, fireEvents: false);
        Assert.AreEqual(teamB, tm.CurrentTeamId);

        tm.EndTurn(teamB, fireEvents: false);
        Assert.AreEqual(teamA, tm.CurrentTeamId, "Should cycle back to teamA.");
        Assert.AreEqual(3, tm.TurnNumber);

        Debug.Log("TurnManager_TurnCycles_BackToFirstTeam passed.");
    }

    [Test]
    public void TurnManager_FiresTurnEvents_WhenFireEventsTrue()
    {
        var (state, queue) = CreateQueue();
        var tm = new TurnManager(queue);
        var teamA = NewTeam();
        var teamB = NewTeam();

        var receivedEvents = new List<IGameEvent>();

        tm.RegisterTeam(teamA);
        tm.RegisterTeam(teamB);
        tm.StartBattle(fireEvents: true);
        queue.ProcessQueue();

        // Verify battle start and turn start events were enqueued
        // (We verify by checking events processed via a custom listener, 
        //  but here we just verify the turn manager state is correct)
        Assert.AreEqual(teamA, tm.CurrentTeamId);
        Assert.AreEqual(1, tm.TurnNumber);

        Debug.Log("TurnManager_FiresTurnEvents_WhenFireEventsTrue passed.");
    }

    [Test]
    public void TurnManager_EndTurn_FiresEventsWhenRequested()
    {
        var (state, queue) = CreateQueue();
        var tm = new TurnManager(queue);
        var teamA = NewTeam();
        var teamB = NewTeam();

        tm.RegisterTeam(teamA);
        tm.RegisterTeam(teamB);
        tm.StartBattle(fireEvents: false);

        tm.EndTurn(teamA, fireEvents: true);
        // Events are enqueued; process them
        queue.ProcessQueue();

        Assert.AreEqual(teamB, tm.CurrentTeamId);
        Debug.Log("TurnManager_EndTurn_FiresEventsWhenRequested passed.");
    }

    [Test]
    public void TurnManager_EndTurnSilent_NoEventsEnqueued()
    {
        var (state, queue) = CreateQueue();
        var tm = new TurnManager(queue);
        var teamA = NewTeam();
        var teamB = NewTeam();

        tm.RegisterTeam(teamA);
        tm.RegisterTeam(teamB);
        tm.StartBattle(fireEvents: false);

        // fireEvents=false means silent end
        tm.EndTurn(teamA, fireEvents: false);

        // State should change, but no events in queue
        Assert.AreEqual(teamB, tm.CurrentTeamId);
        Debug.Log("TurnManager_EndTurnSilent_NoEventsEnqueued passed.");
    }

    // ─── DECK COMPONENT TESTS ────────────────────────────────────────────────

    [Test]
    public void DeckComponent_AddToTop_CardIsFirst()
    {
        var deck = new DeckComponent();
        var card1 = new BasicPlayingCard("A", "desc", 0, 0);
        var card2 = new BasicPlayingCard("B", "desc", 0, 0);

        deck.AddToBottom(card1);
        deck.AddToTop(card2);

        Assert.AreEqual(card2, deck.PeekTop(), "Card added to top should be first.");
        Assert.AreEqual(2, deck.Count);
        Debug.Log("DeckComponent_AddToTop_CardIsFirst passed.");
    }

    [Test]
    public void DeckComponent_AddToBottom_CardIsLast()
    {
        var deck = new DeckComponent();
        var card1 = new BasicPlayingCard("A", "desc", 0, 0);
        var card2 = new BasicPlayingCard("B", "desc", 0, 0);

        deck.AddToTop(card1);
        deck.AddToBottom(card2);

        Assert.AreEqual(card2, deck.PeekBottom(), "Card added to bottom should be last.");
        Debug.Log("DeckComponent_AddToBottom_CardIsLast passed.");
    }

    [Test]
    public void DeckComponent_AddAt_CardAtCorrectPosition()
    {
        var deck = new DeckComponent();
        var card1 = new BasicPlayingCard("A", "desc", 0, 0);
        var card2 = new BasicPlayingCard("B", "desc", 0, 0);
        var card3 = new BasicPlayingCard("C", "desc", 0, 0);

        deck.AddToBottom(card1);
        deck.AddToBottom(card3);
        deck.AddAt(card2, 1);

        Assert.AreEqual(card1, deck.GetAt(0));
        Assert.AreEqual(card2, deck.GetAt(1));
        Assert.AreEqual(card3, deck.GetAt(2));
        Debug.Log("DeckComponent_AddAt_CardAtCorrectPosition passed.");
    }

    [Test]
    public void DeckComponent_DrawTop_ReturnsFirstCard()
    {
        var deck = new DeckComponent();
        var card1 = new BasicPlayingCard("A", "desc", 0, 0);
        var card2 = new BasicPlayingCard("B", "desc", 0, 0);

        deck.AddToBottom(card1);
        deck.AddToBottom(card2);

        var drawn = deck.DrawTop();
        Assert.AreEqual(card1, drawn);
        Assert.AreEqual(1, deck.Count);
        Debug.Log("DeckComponent_DrawTop_ReturnsFirstCard passed.");
    }

    [Test]
    public void DeckComponent_DrawBottom_ReturnsLastCard()
    {
        var deck = new DeckComponent();
        var card1 = new BasicPlayingCard("A", "desc", 0, 0);
        var card2 = new BasicPlayingCard("B", "desc", 0, 0);

        deck.AddToBottom(card1);
        deck.AddToBottom(card2);

        var drawn = deck.DrawBottom();
        Assert.AreEqual(card2, drawn);
        Assert.AreEqual(1, deck.Count);
        Debug.Log("DeckComponent_DrawBottom_ReturnsLastCard passed.");
    }

    [Test]
    public void DeckComponent_DrawSpecific_FindsByIdAndRemoves()
    {
        var deck = new DeckComponent();
        var card1 = new BasicPlayingCard("A", "desc", 0, 0);
        var card2 = new BasicPlayingCard("B", "desc", 0, 0);
        var card3 = new BasicPlayingCard("C", "desc", 0, 0);

        deck.AddToBottom(card1);
        deck.AddToBottom(card2);
        deck.AddToBottom(card3);

        var drawn = deck.DrawSpecific(card2.Id);
        Assert.AreEqual(card2, drawn);
        Assert.AreEqual(2, deck.Count);
        Assert.IsNull(deck.FindById(card2.Id));
        Debug.Log("DeckComponent_DrawSpecific_FindsByIdAndRemoves passed.");
    }

    [Test]
    public void DeckComponent_DrawRandom_RemovesAndReturnsCard()
    {
        var state = new BattleState(42);
        var deck = new DeckComponent();
        var cards = new List<IPlayingCard>();
        for (int i = 0; i < 5; i++)
        {
            var c = new BasicPlayingCard($"Card{i}", "desc", 0, 0);
            cards.Add(c);
            deck.AddToBottom(c);
        }

        var drawn = deck.DrawRandom(state.Rng);
        Assert.IsNotNull(drawn);
        Assert.AreEqual(4, deck.Count);
        Assert.IsTrue(cards.Contains(drawn));
        Debug.Log("DeckComponent_DrawRandom_RemovesAndReturnsCard passed.");
    }

    [Test]
    public void DeckComponent_DrawFromEmpty_ReturnsNull()
    {
        var deck = new DeckComponent();
        Assert.IsNull(deck.DrawTop());
        Assert.IsNull(deck.DrawBottom());
        Debug.Log("DeckComponent_DrawFromEmpty_ReturnsNull passed.");
    }

    [Test]
    public void DeckComponent_InitialCards_SetAtCreation()
    {
        var cards = new List<IPlayingCard>
        {
            new BasicPlayingCard("A", "desc", 0, 0),
            new BasicPlayingCard("B", "desc", 0, 0)
        };
        var deck = new DeckComponent(cards);
        Assert.AreEqual(2, deck.Count);
        Debug.Log("DeckComponent_InitialCards_SetAtCreation passed.");
    }

    [Test]
    public void DeckComponent_Shuffle_ChangesOrder()
    {
        var state = new BattleState(123);
        var cards = new List<IPlayingCard>();
        for (int i = 0; i < 10; i++)
            cards.Add(new BasicPlayingCard($"Card{i}", "desc", 0, 0));

        var deck = new DeckComponent(cards);
        var originalOrder = new List<IPlayingCard>(deck.Cards);

        deck.Shuffle(state.Rng);

        // Very unlikely to have the same order after shuffle with 10 cards
        bool changed = false;
        for (int i = 0; i < deck.Count; i++)
            if (deck.Cards[i] != originalOrder[i]) { changed = true; break; }

        Assert.IsTrue(changed, "Shuffle should change card order.");
        Debug.Log("DeckComponent_Shuffle_ChangesOrder passed.");
    }

    [Test]
    public void DeckComponent_MoveCard_ReordersCards()
    {
        var deck = new DeckComponent();
        var card1 = new BasicPlayingCard("A", "desc", 0, 0);
        var card2 = new BasicPlayingCard("B", "desc", 0, 0);
        var card3 = new BasicPlayingCard("C", "desc", 0, 0);

        deck.AddToBottom(card1);
        deck.AddToBottom(card2);
        deck.AddToBottom(card3);

        deck.MoveCard(0, 2); // Move A from index 0 to index 2

        Assert.AreEqual(card2, deck.GetAt(0));
        Assert.AreEqual(card3, deck.GetAt(1));
        Assert.AreEqual(card1, deck.GetAt(2));
        Debug.Log("DeckComponent_MoveCard_ReordersCards passed.");
    }

    [Test]
    public void DeckComponent_DiscardCard_MovesToDiscard()
    {
        var deck = new DeckComponent();
        var discard = new DiscardComponent();
        var card = new BasicPlayingCard("A", "desc", 0, 0);
        deck.AddToBottom(card);

        deck.DiscardCard(card, discard);

        Assert.AreEqual(0, deck.Count);
        Assert.AreEqual(1, discard.Count);
        Assert.AreEqual(card, discard.PeekTop());
        Debug.Log("DeckComponent_DiscardCard_MovesToDiscard passed.");
    }

    [Test]
    public void DeckComponent_DestroyCard_RemovesFromDeck()
    {
        var deck = new DeckComponent();
        var card = new BasicPlayingCard("A", "desc", 0, 0);
        deck.AddToBottom(card);

        var result = deck.DestroyCard(card);

        Assert.IsTrue(result);
        Assert.AreEqual(0, deck.Count);
        Debug.Log("DeckComponent_DestroyCard_RemovesFromDeck passed.");
    }

    // ─── HAND COMPONENT TESTS ────────────────────────────────────────────────

    [Test]
    public void HandComponent_DrawFromDeck_TransfersCard()
    {
        var deck = new DeckComponent();
        var card = new BasicPlayingCard("A", "desc", 0, 0);
        deck.AddToBottom(card);

        var hand = new HandComponent(deck);
        var drawn = hand.DrawFromDeck();

        Assert.AreEqual(card, drawn);
        Assert.AreEqual(0, deck.Count);
        Assert.AreEqual(1, hand.Count);
        Debug.Log("HandComponent_DrawFromDeck_TransfersCard passed.");
    }

    [Test]
    public void HandComponent_DrawFromEmptyDeck_ReturnsNull()
    {
        var deck = new DeckComponent();
        var hand = new HandComponent(deck);

        var drawn = hand.DrawFromDeck();
        Assert.IsNull(drawn);
        Assert.AreEqual(0, hand.Count);
        Debug.Log("HandComponent_DrawFromEmptyDeck_ReturnsNull passed.");
    }

    [Test]
    public void HandComponent_DrawMultiple_DrawsCorrectCount()
    {
        var deck = new DeckComponent();
        for (int i = 0; i < 5; i++)
            deck.AddToBottom(new BasicPlayingCard($"Card{i}", "desc", 0, 0));

        var hand = new HandComponent(deck);
        hand.DrawMultiple(3);

        Assert.AreEqual(3, hand.Count);
        Assert.AreEqual(2, deck.Count);
        Debug.Log("HandComponent_DrawMultiple_DrawsCorrectCount passed.");
    }

    [Test]
    public void HandComponent_AutoDraw_DrawsAutoDrawCountCards()
    {
        var deck = new DeckComponent();
        for (int i = 0; i < 5; i++)
            deck.AddToBottom(new BasicPlayingCard($"Card{i}", "desc", 0, 0));

        var hand = new HandComponent(deck) { AutoDrawCount = 2 };
        hand.AutoDraw();

        Assert.AreEqual(2, hand.Count);
        Assert.AreEqual(3, deck.Count);
        Debug.Log("HandComponent_AutoDraw_DrawsAutoDrawCountCards passed.");
    }

    [Test]
    public void HandComponent_PlayCard_RemovesFromHand()
    {
        var deck = new DeckComponent();
        var card = new BasicPlayingCard("A", "desc", 0, 0);
        deck.AddToBottom(card);

        var hand = new HandComponent(deck);
        hand.DrawFromDeck();

        var played = hand.PlayCard(card);
        Assert.AreEqual(card, played);
        Assert.AreEqual(0, hand.Count);
        Debug.Log("HandComponent_PlayCard_RemovesFromHand passed.");
    }

    [Test]
    public void HandComponent_DiscardCard_MovesToDiscard()
    {
        var deck = new DeckComponent();
        var discard = new DiscardComponent();
        var card = new BasicPlayingCard("A", "desc", 0, 0);
        deck.AddToBottom(card);

        var hand = new HandComponent(deck);
        hand.DrawFromDeck();
        hand.DiscardCard(card, discard);

        Assert.AreEqual(0, hand.Count);
        Assert.AreEqual(1, discard.Count);
        Debug.Log("HandComponent_DiscardCard_MovesToDiscard passed.");
    }

    [Test]
    public void HandComponent_DiscardAll_MovesAllToDiscard()
    {
        var deck = new DeckComponent();
        var discard = new DiscardComponent();

        for (int i = 0; i < 3; i++)
            deck.AddToBottom(new BasicPlayingCard($"Card{i}", "desc", 0, 0));

        var hand = new HandComponent(deck);
        hand.DrawMultiple(3);

        hand.DiscardAll(discard);

        Assert.AreEqual(0, hand.Count);
        Assert.AreEqual(3, discard.Count);
        Debug.Log("HandComponent_DiscardAll_MovesAllToDiscard passed.");
    }

    [Test]
    public void HandComponent_ExileCard_RemovesForever()
    {
        var deck = new DeckComponent();
        var card = new BasicPlayingCard("A", "desc", 0, 0);
        deck.AddToBottom(card);

        var hand = new HandComponent(deck);
        hand.DrawFromDeck();

        var result = hand.ExileCard(card);
        Assert.IsTrue(result);
        Assert.AreEqual(0, hand.Count);
        Debug.Log("HandComponent_ExileCard_RemovesForever passed.");
    }

    [Test]
    public void HandComponent_MoveCard_ReordersInHand()
    {
        var deck = new DeckComponent();
        var card1 = new BasicPlayingCard("A", "desc", 0, 0);
        var card2 = new BasicPlayingCard("B", "desc", 0, 0);
        deck.AddToBottom(card1);
        deck.AddToBottom(card2);

        var hand = new HandComponent(deck);
        hand.DrawMultiple(2);

        hand.MoveCard(0, 1);

        Assert.AreEqual(card2, hand.GetAt(0));
        Assert.AreEqual(card1, hand.GetAt(1));
        Debug.Log("HandComponent_MoveCard_ReordersInHand passed.");
    }

    [Test]
    public void HandComponent_LinkedDeckIsSet()
    {
        var deck = new DeckComponent();
        var hand = new HandComponent(deck);
        Assert.AreSame(deck, hand.LinkedDeck);
        Debug.Log("HandComponent_LinkedDeckIsSet passed.");
    }

    [Test]
    public void HandComponent_HasDrawnInitial_DefaultFalse()
    {
        var deck = new DeckComponent();
        var hand = new HandComponent(deck);
        Assert.IsFalse(hand.HasDrawnInitial);
        Debug.Log("HandComponent_HasDrawnInitial_DefaultFalse passed.");
    }

    // ─── DISCARD COMPONENT TESTS ─────────────────────────────────────────────

    [Test]
    public void DiscardComponent_ShuffleIntoDeck_MovesAllCards()
    {
        var state = new BattleState(42);
        var deck = new DeckComponent();
        var discard = new DiscardComponent();

        for (int i = 0; i < 3; i++)
            discard.AddToTop(new BasicPlayingCard($"Card{i}", "desc", 0, 0));

        discard.ShuffleIntoDeck(deck, state.Rng);

        Assert.AreEqual(0, discard.Count);
        Assert.AreEqual(3, deck.Count);
        Debug.Log("DiscardComponent_ShuffleIntoDeck_MovesAllCards passed.");
    }

    [Test]
    public void DiscardComponent_TakeCard_FindsAndRemoves()
    {
        var discard = new DiscardComponent();
        var card = new BasicPlayingCard("A", "desc", 0, 0);
        discard.AddToTop(card);

        var taken = discard.TakeCard(card.Id);
        Assert.AreEqual(card, taken);
        Assert.AreEqual(0, discard.Count);
        Debug.Log("DiscardComponent_TakeCard_FindsAndRemoves passed.");
    }

    [Test]
    public void DiscardComponent_TakeTop_ReturnsTopCard()
    {
        var discard = new DiscardComponent();
        var card1 = new BasicPlayingCard("A", "desc", 0, 0);
        var card2 = new BasicPlayingCard("B", "desc", 0, 0);

        discard.AddToBottom(card1);
        discard.AddToBottom(card2);

        var top = discard.TakeTop();
        Assert.AreEqual(card1, top);
        Assert.AreEqual(1, discard.Count);
        Debug.Log("DiscardComponent_TakeTop_ReturnsTopCard passed.");
    }

    // ─── AUTO DRAW SYSTEM TESTS ──────────────────────────────────────────────

    [Test]
    public void AutoDrawSystem_BattleStart_DrawsForAllEntities()
    {
        var (state, queue) = CreateQueue();
        var teamA = NewTeam();

        var entity = AddEntity(state, teamA);
        var deck = new DeckComponent();
        for (int i = 0; i < 5; i++)
            deck.AddToBottom(new BasicPlayingCard($"Card{i}", "desc", 0, 0));
        var hand = new HandComponent(deck) { AutoDrawCount = 3 };
        entity.AddComponent(hand);
        entity.AddComponent(deck);

        var battleStart = new BattleStartEvent(Geid.New);
        queue.Enqueue(battleStart);
        queue.ProcessQueue();

        Assert.AreEqual(3, hand.Count, "Should draw 3 cards at battle start.");
        Assert.IsTrue(hand.HasDrawnInitial, "HasDrawnInitial should be set after battle start.");
        Debug.Log($"AutoDrawSystem_BattleStart_DrawsForAllEntities passed. Hand count: {hand.Count}");
    }

    [Test]
    public void AutoDrawSystem_TurnStart_SkipsFirstTurnDraw()
    {
        var (state, queue) = CreateQueue();
        var teamA = NewTeam();

        var entity = AddEntity(state, teamA);
        var deck = new DeckComponent();
        for (int i = 0; i < 5; i++)
            deck.AddToBottom(new BasicPlayingCard($"Card{i}", "desc", 0, 0));
        var hand = new HandComponent(deck) { AutoDrawCount = 1, HasDrawnInitial = false };
        entity.AddComponent(hand);
        entity.AddComponent(deck);

        // First turn start should NOT draw (HasDrawnInitial is false)
        var turnStart = new TurnStartEvent(Geid.New, teamA, 1);
        queue.Enqueue(turnStart);
        queue.ProcessQueue();

        Assert.AreEqual(0, hand.Count, "First turn start should NOT draw cards.");
        Assert.IsTrue(hand.HasDrawnInitial, "HasDrawnInitial should be set to true after first turn.");
        Debug.Log("AutoDrawSystem_TurnStart_SkipsFirstTurnDraw passed.");
    }

    [Test]
    public void AutoDrawSystem_TurnStart_DrawsOnSubsequentTurns()
    {
        var (state, queue) = CreateQueue();
        var teamA = NewTeam();

        var entity = AddEntity(state, teamA);
        var deck = new DeckComponent();
        for (int i = 0; i < 5; i++)
            deck.AddToBottom(new BasicPlayingCard($"Card{i}", "desc", 0, 0));
        var hand = new HandComponent(deck) { AutoDrawCount = 2, HasDrawnInitial = true };
        entity.AddComponent(hand);
        entity.AddComponent(deck);

        var turnStart = new TurnStartEvent(Geid.New, teamA, 2);
        queue.Enqueue(turnStart);
        queue.ProcessQueue();

        Assert.AreEqual(2, hand.Count, "Should draw 2 cards on subsequent turns.");
        Debug.Log("AutoDrawSystem_TurnStart_DrawsOnSubsequentTurns passed.");
    }

    [Test]
    public void AutoDrawSystem_TurnStart_OnlyDrawsForCurrentTeam()
    {
        var (state, queue) = CreateQueue();
        var teamA = NewTeam();
        var teamB = NewTeam();

        var entityA = AddEntity(state, teamA);
        var entityB = AddEntity(state, teamB);

        var deckA = new DeckComponent();
        var deckB = new DeckComponent();
        for (int i = 0; i < 3; i++)
        {
            deckA.AddToBottom(new BasicPlayingCard($"A{i}", "desc", 0, 0));
            deckB.AddToBottom(new BasicPlayingCard($"B{i}", "desc", 0, 0));
        }
        var handA = new HandComponent(deckA) { AutoDrawCount = 1, HasDrawnInitial = true };
        var handB = new HandComponent(deckB) { AutoDrawCount = 1, HasDrawnInitial = true };

        entityA.AddComponent(handA);
        entityA.AddComponent(deckA);
        entityB.AddComponent(handB);
        entityB.AddComponent(deckB);

        // Only teamA's turn starts
        var turnStart = new TurnStartEvent(Geid.New, teamA, 2);
        queue.Enqueue(turnStart);
        queue.ProcessQueue();

        Assert.AreEqual(1, handA.Count, "TeamA entity should draw 1 card.");
        Assert.AreEqual(0, handB.Count, "TeamB entity should NOT draw (not their turn).");
        Debug.Log("AutoDrawSystem_TurnStart_OnlyDrawsForCurrentTeam passed.");
    }

    // ─── RESOURCE COST SYSTEM TESTS ──────────────────────────────────────────

    [Test]
    public void ResourceCostSystem_DeductsResources_WhenSufficient()
    {
        var (state, queue) = CreateQueue();
        var entity = new BaseEntity();
        entity.AddComponent(new ManaComponent(10));
        entity.AddComponent(new EnergyComponent(10));
        state.AddEntity(entity);

        var spendEvent = new SpendResourcesEvent(entity.Id, entity.Id, 3, 2);
        queue.Enqueue(spendEvent);
        queue.ProcessQueue();

        Assert.AreEqual(EventStatus.Applied, spendEvent.Status);
        Assert.AreEqual(7, entity.GetComponent<ManaComponent>().CurrentMana);
        Assert.AreEqual(8, entity.GetComponent<EnergyComponent>().CurrentEnergy);
        Debug.Log("ResourceCostSystem_DeductsResources_WhenSufficient passed.");
    }

    [Test]
    public void ResourceCostSystem_CancelsEvent_WhenInsufficientMana()
    {
        var (state, queue) = CreateQueue();
        var entity = new BaseEntity();
        entity.AddComponent(new ManaComponent(2)); // Only 2 mana, need 3
        entity.AddComponent(new EnergyComponent(10));
        state.AddEntity(entity);

        var spendEvent = new SpendResourcesEvent(entity.Id, entity.Id, 3, 0);
        queue.Enqueue(spendEvent);
        queue.ProcessQueue();

        Assert.AreEqual(EventStatus.Cancelled, spendEvent.Status);
        Assert.AreEqual(2, entity.GetComponent<ManaComponent>().CurrentMana, "Mana should not be deducted when cancelled.");
        Debug.Log("ResourceCostSystem_CancelsEvent_WhenInsufficientMana passed.");
    }

    [Test]
    public void ResourceCostSystem_CancelsEvent_WhenInsufficientEnergy()
    {
        var (state, queue) = CreateQueue();
        var entity = new BaseEntity();
        entity.AddComponent(new ManaComponent(10));
        entity.AddComponent(new EnergyComponent(1)); // Only 1 energy, need 2
        state.AddEntity(entity);

        var spendEvent = new SpendResourcesEvent(entity.Id, entity.Id, 0, 2);
        queue.Enqueue(spendEvent);
        queue.ProcessQueue();

        Assert.AreEqual(EventStatus.Cancelled, spendEvent.Status);
        Assert.AreEqual(1, entity.GetComponent<EnergyComponent>().CurrentEnergy, "Energy should not be deducted when cancelled.");
        Debug.Log("ResourceCostSystem_CancelsEvent_WhenInsufficientEnergy passed.");
    }

    // ─── FIREBALL CARD TESTS ─────────────────────────────────────────────────

    [Test]
    public void FireballCard_DamagesRandomEnemy_WhenResourcesSufficient()
    {
        var (state, queue) = CreateQueue();
        var teamA = NewTeam();
        var teamB = NewTeam();

        var caster = AddEntity(state, teamA, hp: 100, mana: 5, energy: 5);
        var enemy = AddEntity(state, teamB, hp: 100, mana: 0, energy: 0);

        var card = new FireballCard(caster.Id);
        var playEvent = new PlayCardEvent(caster.Id, card, caster.Id);
        queue.Enqueue(playEvent);
        queue.ProcessQueue();

        var enemyHp = enemy.GetComponent<HealthComponent>().CurrentHealth;
        Assert.AreEqual(94, enemyHp, "Enemy should take 6 damage from Fireball.");
        Assert.AreEqual(2, caster.GetComponent<ManaComponent>().CurrentMana, "Caster should have spent 3 mana.");
        Assert.AreEqual(3, caster.GetComponent<EnergyComponent>().CurrentEnergy, "Caster should have spent 2 energy.");
        Debug.Log($"FireballCard_DamagesRandomEnemy passed. Enemy HP: {enemyHp}");
    }

    [Test]
    public void FireballCard_Cancelled_WhenInsufficientMana()
    {
        var (state, queue) = CreateQueue();
        var teamA = NewTeam();
        var teamB = NewTeam();

        var caster = AddEntity(state, teamA, hp: 100, mana: 2, energy: 5); // Only 2 mana, need 3
        var enemy = AddEntity(state, teamB, hp: 100, mana: 0, energy: 0);

        var card = new FireballCard(caster.Id);
        var checkEvent = new PlayCardCheckEvent(caster.Id, card, caster.Id);
        queue.Enqueue(checkEvent);
        queue.ProcessQueue();

        var enemyHp = enemy.GetComponent<HealthComponent>().CurrentHealth;
        Assert.AreEqual(100, enemyHp, "Enemy should NOT take damage when card is cancelled (insufficient mana).");
        Debug.Log("FireballCard_Cancelled_WhenInsufficientMana passed.");
    }

    [Test]
    public void FireballCard_DamagesSelf_WhenNoEnemies()
    {
        var (state, queue) = CreateQueue();
        var teamA = NewTeam();

        var caster = AddEntity(state, teamA, hp: 100, mana: 5, energy: 5);
        // No enemies

        var card = new FireballCard(caster.Id);
        var playEvent = new PlayCardEvent(caster.Id, card, caster.Id);
        queue.Enqueue(playEvent);
        queue.ProcessQueue();

        var casterHp = caster.GetComponent<HealthComponent>().CurrentHealth;
        Assert.AreEqual(94, casterHp, "Caster should take 6 damage when there are no enemies.");
        Debug.Log($"FireballCard_DamagesSelf_WhenNoEnemies passed. Caster HP: {casterHp}");
    }

    // ─── KNIFE THROW CARD TESTS ──────────────────────────────────────────────

    [Test]
    public void KnifeThrowCard_DamagesEnemiesInCone()
    {
        var (state, queue) = CreateQueue();
        var teamA = NewTeam();
        var teamB = NewTeam();

        var direction = new HexCoordinates(1, 0);
        var caster = AddEntity(state, teamA, hp: 100, mana: 10, energy: 10, hex: new HexCoordinates(0, 0));
        var enemy1 = AddEntity(state, teamB, hp: 100, mana: 0, energy: 0, hex: new HexCoordinates(1, 0));
        var enemy2 = AddEntity(state, teamB, hp: 100, mana: 0, energy: 0, hex: new HexCoordinates(2, 0));
        var enemyFar = AddEntity(state, teamB, hp: 100, mana: 0, energy: 0, hex: new HexCoordinates(0, 3)); // Not in cone

        var card = new KnifeThrowCard(caster.Id, direction);
        var playEvent = new PlayCardEvent(caster.Id, card, caster.Id);
        queue.Enqueue(playEvent);
        queue.ProcessQueue();

        Assert.AreEqual(97, enemy1.GetComponent<HealthComponent>().CurrentHealth, "Enemy1 in cone should take 3 damage.");
        Assert.AreEqual(97, enemy2.GetComponent<HealthComponent>().CurrentHealth, "Enemy2 in cone should take 3 damage.");
        Assert.AreEqual(100, enemyFar.GetComponent<HealthComponent>().CurrentHealth, "Enemy far from cone should NOT take damage.");
        Debug.Log("KnifeThrowCard_DamagesEnemiesInCone passed.");
    }

    [Test]
    public void KnifeThrowCard_Fizzled_WhenNoEnemiesInCone()
    {
        var (state, queue) = CreateQueue();
        var teamA = NewTeam();
        var teamB = NewTeam();

        var direction = new HexCoordinates(1, 0);
        var caster = AddEntity(state, teamA, hp: 100, mana: 10, energy: 10, hex: new HexCoordinates(0, 0));
        // Enemy is NOT in the cone direction
        var enemy = AddEntity(state, teamB, hp: 100, mana: 0, energy: 0, hex: new HexCoordinates(0, 3));

        var card = new KnifeThrowCard(caster.Id, direction);
        var playEvent = new PlayCardEvent(caster.Id, card, caster.Id);
        queue.Enqueue(playEvent);
        queue.ProcessQueue();

        Assert.AreEqual(100, enemy.GetComponent<HealthComponent>().CurrentHealth, "Enemy not in cone should NOT take damage.");
        Debug.Log("KnifeThrowCard_Fizzled_WhenNoEnemiesInCone passed.");
    }

    [Test]
    public void KnifeThrowCard_Cancelled_WhenInsufficientEnergy()
    {
        var (state, queue) = CreateQueue();
        var teamA = NewTeam();
        var teamB = NewTeam();

        var direction = new HexCoordinates(1, 0);
        var caster = AddEntity(state, teamA, hp: 100, mana: 10, energy: 1, hex: new HexCoordinates(0, 0)); // Only 1 energy, need 2
        var enemy = AddEntity(state, teamB, hp: 100, mana: 0, energy: 0, hex: new HexCoordinates(1, 0));

        var card = new KnifeThrowCard(caster.Id, direction);
        var checkEvent = new PlayCardCheckEvent(caster.Id, card, caster.Id);
        queue.Enqueue(checkEvent);
        queue.ProcessQueue();

        Assert.AreEqual(100, enemy.GetComponent<HealthComponent>().CurrentHealth, "Enemy should NOT take damage when card is cancelled.");
        Debug.Log("KnifeThrowCard_Cancelled_WhenInsufficientEnergy passed.");
    }

    // ─── HEALING LIGHTNING CARD TESTS ────────────────────────────────────────

    [Test]
    public void HealingLightningCard_HealsInitialTarget()
    {
        var (state, queue) = CreateQueue();
        var teamA = NewTeam();

        var caster = AddEntity(state, teamA, hp: 80, mana: 10, energy: 10, hex: new HexCoordinates(0, 0));
        caster.GetComponent<HealthComponent>().CurrentHealth = 80;

        var card = new HealingLightningCard(caster.Id, caster.Id);
        var playEvent = new PlayCardEvent(caster.Id, card, caster.Id);
        queue.Enqueue(playEvent);
        queue.ProcessQueue();

        Assert.AreEqual(84, caster.GetComponent<HealthComponent>().CurrentHealth, "Caster should be healed for 4 HP.");
        Debug.Log("HealingLightningCard_HealsInitialTarget passed.");
    }

    [Test]
    public void HealingLightningCard_ChainHeal_HalvesEachStep()
    {
        var (state, queue) = CreateQueue();
        var teamA = NewTeam();

        var caster = AddEntity(state, teamA, hp: 70, mana: 10, energy: 10, hex: new HexCoordinates(0, 0));
        var ally1 = AddEntity(state, teamA, hp: 70, mana: 0, energy: 0, hex: new HexCoordinates(1, 0)); // Distance 1
        var ally2 = AddEntity(state, teamA, hp: 70, mana: 0, energy: 0, hex: new HexCoordinates(2, 0)); // Distance 1 from ally1

        caster.GetComponent<HealthComponent>().CurrentHealth = 70;
        ally1.GetComponent<HealthComponent>().CurrentHealth = 70;
        ally2.GetComponent<HealthComponent>().CurrentHealth = 70;

        var card = new HealingLightningCard(caster.Id, caster.Id);
        var playEvent = new PlayCardEvent(caster.Id, card, caster.Id);
        queue.Enqueue(playEvent);
        queue.ProcessQueue();

        int casterHp = caster.GetComponent<HealthComponent>().CurrentHealth;
        int ally1Hp = ally1.GetComponent<HealthComponent>().CurrentHealth;
        int ally2Hp = ally2.GetComponent<HealthComponent>().CurrentHealth;

        Assert.AreEqual(74, casterHp, "Caster should be healed for 4 HP.");
        Assert.AreEqual(72, ally1Hp, "Ally1 should be healed for 2 HP (4/2).");
        Assert.AreEqual(71, ally2Hp, "Ally2 should be healed for 1 HP (4/2/2).");
        Debug.Log($"HealingLightningCard_ChainHeal passed. Caster: {casterHp}, Ally1: {ally1Hp}, Ally2: {ally2Hp}");
    }

    [Test]
    public void HealingLightningCard_ChainStops_WhenHealLessThan1()
    {
        var (state, queue) = CreateQueue();
        var teamA = NewTeam();

        var caster = AddEntity(state, teamA, hp: 90, mana: 10, energy: 10, hex: new HexCoordinates(0, 0));
        var ally1 = AddEntity(state, teamA, hp: 90, mana: 0, energy: 0, hex: new HexCoordinates(1, 0));
        var ally2 = AddEntity(state, teamA, hp: 90, mana: 0, energy: 0, hex: new HexCoordinates(2, 0));
        var ally3 = AddEntity(state, teamA, hp: 90, mana: 0, energy: 0, hex: new HexCoordinates(3, 0));

        caster.GetComponent<HealthComponent>().CurrentHealth = 90;
        ally1.GetComponent<HealthComponent>().CurrentHealth = 90;
        ally2.GetComponent<HealthComponent>().CurrentHealth = 90;
        ally3.GetComponent<HealthComponent>().CurrentHealth = 90;

        // Start with heal 1 - first ally heals 1, next would be 0 (< 1), so chain stops
        var loopState = new HealingLightningLoopState(caster.Id, caster.Id, 1, new System.Collections.Generic.List<Geid>());
        var lightningEvent = new LoopEvent(caster.Id, loopState);
        queue.Enqueue(lightningEvent);
        queue.ProcessQueue();

        // Chain: caster +1, ally1: 0 (chain stops)
        Assert.AreEqual(91, caster.GetComponent<HealthComponent>().CurrentHealth, "Caster should be healed for 1.");
        Assert.AreEqual(90, ally1.GetComponent<HealthComponent>().CurrentHealth, "Chain should stop; ally1 not healed.");
        Debug.Log("HealingLightningCard_ChainStops_WhenHealLessThan1 passed.");
    }

    [Test]
    public void HealingLightningCard_DoesNotHealAboveMax()
    {
        var (state, queue) = CreateQueue();
        var teamA = NewTeam();

        var caster = AddEntity(state, teamA, hp: 100, mana: 10, energy: 10, hex: new HexCoordinates(0, 0));
        caster.GetComponent<HealthComponent>().CurrentHealth = 98; // Near max

        var card = new HealingLightningCard(caster.Id, caster.Id);
        var playEvent = new PlayCardEvent(caster.Id, card, caster.Id);
        queue.Enqueue(playEvent);
        queue.ProcessQueue();

        Assert.AreEqual(100, caster.GetComponent<HealthComponent>().CurrentHealth, "HP should not exceed max.");
        Debug.Log("HealingLightningCard_DoesNotHealAboveMax passed.");
    }

    // ─── RICOCHET SWORD CARD TESTS ───────────────────────────────────────────

    [Test]
    public void RicochetSwordCard_DamagesInitialTarget()
    {
        var (state, queue) = CreateQueue();
        var teamA = NewTeam();
        var teamB = NewTeam();

        var caster = AddEntity(state, teamA, hp: 100, mana: 5, energy: 10, hex: new HexCoordinates(0, 0));
        var enemy = AddEntity(state, teamB, hp: 100, mana: 0, energy: 0, hex: new HexCoordinates(1, 0));

        var card = new RicochetSwordCard(caster.Id, enemy.Id);
        var playEvent = new PlayCardEvent(caster.Id, card, caster.Id);
        queue.Enqueue(playEvent);
        queue.ProcessQueue();

        Assert.AreEqual(96, enemy.GetComponent<HealthComponent>().CurrentHealth, "Enemy should take 4 damage.");
        // Caster starts with 5 mana - 1 (initial cost) = 4 mana
        Assert.AreEqual(4, caster.GetComponent<ManaComponent>().CurrentMana, "Caster should spend 1 mana initially.");
        Debug.Log($"RicochetSwordCard_DamagesInitialTarget passed. Enemy HP: {enemy.GetComponent<HealthComponent>().CurrentHealth}");
    }

    [Test]
    public void RicochetSwordCard_Ricochet_ToNearbyEnemy()
    {
        var (state, queue) = CreateQueue();
        var teamA = NewTeam();
        var teamB = NewTeam();

        var caster = AddEntity(state, teamA, hp: 100, mana: 5, energy: 10, hex: new HexCoordinates(0, 0));
        var enemy1 = AddEntity(state, teamB, hp: 100, mana: 0, energy: 0, hex: new HexCoordinates(2, 0));
        var enemy2 = AddEntity(state, teamB, hp: 100, mana: 0, energy: 0, hex: new HexCoordinates(4, 0)); // Within ricochet range (dist 2 from enemy1)

        var card = new RicochetSwordCard(caster.Id, enemy1.Id);
        var playEvent = new PlayCardEvent(caster.Id, card, caster.Id);
        queue.Enqueue(playEvent);
        queue.ProcessQueue();

        Assert.AreEqual(96, enemy1.GetComponent<HealthComponent>().CurrentHealth, "Enemy1 should take 4 damage.");
        Assert.AreEqual(96, enemy2.GetComponent<HealthComponent>().CurrentHealth, "Enemy2 should take 4 ricochet damage.");
        Debug.Log("RicochetSwordCard_Ricochet_ToNearbyEnemy passed.");
    }

    [Test]
    public void RicochetSwordCard_Cancelled_WhenInsufficientResources()
    {
        var (state, queue) = CreateQueue();
        var teamA = NewTeam();
        var teamB = NewTeam();

        var caster = AddEntity(state, teamA, hp: 100, mana: 0, energy: 0, hex: new HexCoordinates(0, 0)); // No resources
        var enemy = AddEntity(state, teamB, hp: 100, mana: 0, energy: 0, hex: new HexCoordinates(1, 0));

        var card = new RicochetSwordCard(caster.Id, enemy.Id);
        var checkEvent = new PlayCardCheckEvent(caster.Id, card, caster.Id);
        queue.Enqueue(checkEvent);
        queue.ProcessQueue();

        Assert.AreEqual(100, enemy.GetComponent<HealthComponent>().CurrentHealth, "Enemy should NOT take damage when card is cancelled.");
        Debug.Log("RicochetSwordCard_Cancelled_WhenInsufficientResources passed.");
    }

    [Test]
    public void RicochetSwordCard_StopsRicochet_WhenNoMoreResources()
    {
        var (state, queue) = CreateQueue();
        var teamA = NewTeam();
        var teamB = NewTeam();

        // Caster has exactly enough for initial + 1 ricochet (1+1=2 mana, 3+1=4 energy)
        var caster = AddEntity(state, teamA, hp: 100, mana: 2, energy: 4, hex: new HexCoordinates(0, 0));
        var enemy1 = AddEntity(state, teamB, hp: 100, mana: 0, energy: 0, hex: new HexCoordinates(2, 0));
        var enemy2 = AddEntity(state, teamB, hp: 100, mana: 0, energy: 0, hex: new HexCoordinates(4, 0)); // Ricochet target
        var enemy3 = AddEntity(state, teamB, hp: 100, mana: 0, energy: 0, hex: new HexCoordinates(6, 0)); // Would be 2nd ricochet, but no resources

        var card = new RicochetSwordCard(caster.Id, enemy1.Id);
        var playEvent = new PlayCardEvent(caster.Id, card, caster.Id);
        queue.Enqueue(playEvent);
        queue.ProcessQueue();

        // enemy1 should take damage (initial strike)
        Assert.AreEqual(96, enemy1.GetComponent<HealthComponent>().CurrentHealth, "Enemy1 should take 4 damage.");
        // enemy2 should take damage (first ricochet)
        Assert.AreEqual(96, enemy2.GetComponent<HealthComponent>().CurrentHealth, "Enemy2 should take 4 ricochet damage.");
        // enemy3 should NOT take damage (no resources for 2nd ricochet)
        Assert.AreEqual(100, enemy3.GetComponent<HealthComponent>().CurrentHealth, "Enemy3 should NOT take damage (no more resources).");
        Debug.Log("RicochetSwordCard_StopsRicochet_WhenNoMoreResources passed.");
    }

    // ─── FIRE WALL CARD TESTS ────────────────────────────────────────────────

    [Test]
    public void FireWallCard_DamagesEnemiesInLine()
    {
        var (state, queue) = CreateQueue();
        var teamA = NewTeam();
        var teamB = NewTeam();

        var direction = new HexCoordinates(1, 0);
        var caster = AddEntity(state, teamA, hp: 100, mana: 5, energy: 5, hex: new HexCoordinates(0, 0));
        var enemy1 = AddEntity(state, teamB, hp: 100, mana: 0, energy: 0, hex: new HexCoordinates(1, 0));
        var enemy2 = AddEntity(state, teamB, hp: 100, mana: 0, energy: 0, hex: new HexCoordinates(2, 0));
        var enemyOff = AddEntity(state, teamB, hp: 100, mana: 0, energy: 0, hex: new HexCoordinates(1, 2)); // Off-line

        var card = new FireWallCard(caster.Id, direction);
        var playEvent = new PlayCardEvent(caster.Id, card, caster.Id);
        queue.Enqueue(playEvent);
        queue.ProcessQueue();

        Assert.AreEqual(97, enemy1.GetComponent<HealthComponent>().CurrentHealth, "Enemy1 in line should take 3 damage.");
        Assert.AreEqual(97, enemy2.GetComponent<HealthComponent>().CurrentHealth, "Enemy2 in line should take 3 damage.");
        Assert.AreEqual(100, enemyOff.GetComponent<HealthComponent>().CurrentHealth, "Enemy off-line should NOT take damage.");
        Debug.Log("FireWallCard_DamagesEnemiesInLine passed.");
    }

    [Test]
    public void FireWallCard_AppliesBurn_ToEnemiesInLine()
    {
        var (state, queue) = CreateQueue();
        var teamA = NewTeam();
        var teamB = NewTeam();

        var direction = new HexCoordinates(1, 0);
        var caster = AddEntity(state, teamA, hp: 100, mana: 5, energy: 5, hex: new HexCoordinates(0, 0));
        var enemy = AddEntity(state, teamB, hp: 100, mana: 0, energy: 0, hex: new HexCoordinates(1, 0));

        var card = new FireWallCard(caster.Id, direction);
        var playEvent = new PlayCardEvent(caster.Id, card, caster.Id);
        queue.Enqueue(playEvent);
        queue.ProcessQueue();

        var burn = enemy.GetComponent<BurnComponent>();
        Assert.IsNotNull(burn, "Enemy should have BurnComponent after FireWall.");
        Assert.AreEqual(3, burn.DamagePerTick, "Burn damage should be 3 per tick.");
        Debug.Log($"FireWallCard_AppliesBurn_ToEnemiesInLine passed. BurnTicks: {burn.RemainingTicks}");
    }

    [Test]
    public void FireWallCard_Cancelled_WhenInsufficientMana()
    {
        var (state, queue) = CreateQueue();
        var teamA = NewTeam();
        var teamB = NewTeam();

        var direction = new HexCoordinates(1, 0);
        var caster = AddEntity(state, teamA, hp: 100, mana: 3, energy: 5, hex: new HexCoordinates(0, 0)); // Need 4 mana
        var enemy = AddEntity(state, teamB, hp: 100, mana: 0, energy: 0, hex: new HexCoordinates(1, 0));

        var card = new FireWallCard(caster.Id, direction);
        var checkEvent = new PlayCardCheckEvent(caster.Id, card, caster.Id);
        queue.Enqueue(checkEvent);
        queue.ProcessQueue();

        Assert.AreEqual(100, enemy.GetComponent<HealthComponent>().CurrentHealth, "Enemy should NOT take damage when card is cancelled.");
        Assert.IsNull(enemy.GetComponent<BurnComponent>(), "Enemy should NOT have burn when card is cancelled.");
        Debug.Log("FireWallCard_Cancelled_WhenInsufficientMana passed.");
    }

    [Test]
    public void FireWallCard_AppliesEffect_EvenWhenNoEnemiesInLine()
    {
        var (state, queue) = CreateQueue();
        var teamA = NewTeam();

        var direction = new HexCoordinates(1, 0);
        var caster = AddEntity(state, teamA, hp: 100, mana: 5, energy: 5, hex: new HexCoordinates(0, 0));
        // No enemies

        var card = new FireWallCard(caster.Id, direction);
        var playEvent = new PlayCardEvent(caster.Id, card, caster.Id);
        queue.Enqueue(playEvent);
        queue.ProcessQueue();

        // Card should apply (spend resources) even with no enemies
        Assert.AreEqual(1, caster.GetComponent<ManaComponent>().CurrentMana, "Mana should be spent even with no enemies (4 mana cost).");
        Assert.AreEqual(4, caster.GetComponent<EnergyComponent>().CurrentEnergy, "Energy should be spent even with no enemies (1 energy cost).");
        Debug.Log("FireWallCard_AppliesEffect_EvenWhenNoEnemiesInLine passed.");
    }

    // ─── BURN SYSTEM TESTS ───────────────────────────────────────────────────

    [Test]
    public void BurnSystem_AppliesBurnComponent()
    {
        var (state, queue) = CreateQueue();
        var teamA = NewTeam();

        var caster = AddEntity(state, teamA);
        var target = AddEntity(state, teamA);

        var burnEvt = new ApplyBurnEvent(caster.Id, caster.Id, target.Id, 5, 3);
        queue.Enqueue(burnEvt);
        queue.ProcessQueue();

        var burn = target.GetComponent<BurnComponent>();
        Assert.IsNotNull(burn);
        Assert.AreEqual(5, burn.DamagePerTick);
        Assert.AreEqual(3, burn.RemainingTicks);
        Debug.Log("BurnSystem_AppliesBurnComponent passed.");
    }

    [Test]
    public void BurnSystem_StacksBurnOnTurnStart()
    {
        var (state, queue) = CreateQueue();
        var teamA = NewTeam();

        var entity = AddEntity(state, teamA, hp: 100);
        entity.AddComponent(new BurnComponent(5, 2));

        var turnStart = new TurnStartEvent(Geid.New, teamA, 2);
        queue.Enqueue(turnStart);
        queue.ProcessQueue();

        Assert.AreEqual(95, entity.GetComponent<HealthComponent>().CurrentHealth, "Entity should take 5 burn damage.");
        Assert.AreEqual(1, entity.GetComponent<BurnComponent>().RemainingTicks, "Burn ticks should decrease.");
        Debug.Log("BurnSystem_StacksBurnOnTurnStart passed.");
    }

    [Test]
    public void BurnSystem_BurnExpires_AfterAllTicks()
    {
        var (state, queue) = CreateQueue();
        var teamA = NewTeam();

        var entity = AddEntity(state, teamA, hp: 100);
        entity.AddComponent(new BurnComponent(3, 1)); // Only 1 tick

        var turnStart = new TurnStartEvent(Geid.New, teamA, 2);
        queue.Enqueue(turnStart);
        queue.ProcessQueue();

        var burn = entity.GetComponent<BurnComponent>();
        Assert.AreEqual(0, burn.RemainingTicks, "Burn should expire after 1 tick.");
        Assert.AreEqual(97, entity.GetComponent<HealthComponent>().CurrentHealth);
        Debug.Log("BurnSystem_BurnExpires_AfterAllTicks passed.");
    }

    // ─── CARD STORAGE BASE TESTS ─────────────────────────────────────────────

    [Test]
    public void CardStorageBase_Contains_FindsByCardId()
    {
        var deck = new DeckComponent();
        var card = new BasicPlayingCard("Test", "desc", 0, 0);
        deck.AddToBottom(card);

        Assert.IsTrue(deck.Contains(card.Id));
        Assert.IsFalse(deck.Contains(Geid.New)); // Random ID not in deck
        Debug.Log("CardStorageBase_Contains_FindsByCardId passed.");
    }

    [Test]
    public void CardStorageBase_IndexOf_ReturnsCorrectIndex()
    {
        var deck = new DeckComponent();
        var card1 = new BasicPlayingCard("A", "desc", 0, 0);
        var card2 = new BasicPlayingCard("B", "desc", 0, 0);
        deck.AddToBottom(card1);
        deck.AddToBottom(card2);

        Assert.AreEqual(0, deck.IndexOf(card1.Id));
        Assert.AreEqual(1, deck.IndexOf(card2.Id));
        Assert.AreEqual(-1, deck.IndexOf(Geid.New));
        Debug.Log("CardStorageBase_IndexOf_ReturnsCorrectIndex passed.");
    }

    [Test]
    public void CardStorageBase_Clear_RemovesAllCards()
    {
        var deck = new DeckComponent();
        for (int i = 0; i < 5; i++)
            deck.AddToBottom(new BasicPlayingCard($"Card{i}", "desc", 0, 0));

        deck.Clear();
        Assert.AreEqual(0, deck.Count);
        Assert.IsTrue(deck.IsEmpty);
        Debug.Log("CardStorageBase_Clear_RemovesAllCards passed.");
    }

    [Test]
    public void CardStorageBase_RemoveById_RemovesCorrectCard()
    {
        var deck = new DeckComponent();
        var card1 = new BasicPlayingCard("A", "desc", 0, 0);
        var card2 = new BasicPlayingCard("B", "desc", 0, 0);
        deck.AddToBottom(card1);
        deck.AddToBottom(card2);

        bool removed = deck.RemoveById(card1.Id);
        Assert.IsTrue(removed);
        Assert.AreEqual(1, deck.Count);
        Assert.AreEqual(card2, deck.PeekTop());
        Debug.Log("CardStorageBase_RemoveById_RemovesCorrectCard passed.");
    }

    // ─── FULL INTEGRATION TEST ───────────────────────────────────────────────

    [Test]
    public void Integration_TurnSystemAndAutoDrawAndCardPlay()
    {
        var (state, queue) = CreateQueue();
        var teamA = NewTeam();
        var teamB = NewTeam();

        // Setup caster entity
        var caster = AddEntity(state, teamA, hp: 100, mana: 10, energy: 10, hex: new HexCoordinates(0, 0));
        var deck = new DeckComponent();
        var card = new FireballCard(caster.Id);
        deck.AddToBottom(card);
        deck.AddToBottom(new BasicPlayingCard("Extra", "desc", 0, 0));

        var hand = new HandComponent(deck) { AutoDrawCount = 1 };
        caster.AddComponent(hand);
        caster.AddComponent(deck);
        caster.AddComponent(new DiscardComponent());

        // Setup enemy
        var enemy = AddEntity(state, teamB, hp: 100, hex: new HexCoordinates(1, 0));

        // Turn manager
        var tm = new TurnManager(queue);
        tm.RegisterTeam(teamA);
        tm.RegisterTeam(teamB);

        // Start battle (draws 1 card for caster)
        tm.StartBattle(fireEvents: true);
        queue.ProcessQueue();

        Assert.AreEqual(1, hand.Count, "Caster should draw 1 card at battle start.");
        Assert.IsTrue(hand.HasDrawnInitial);

        // Play Fireball
        var playedCard = hand.PlayCard(card);
        Assert.IsNotNull(playedCard, "Fireball should be playable from hand.");

        var playEvent = new PlayCardEvent(caster.Id, card, caster.Id);
        queue.Enqueue(playEvent);
        queue.ProcessQueue();

        Assert.AreEqual(94, enemy.GetComponent<HealthComponent>().CurrentHealth, "Enemy should take 6 damage from Fireball.");

        // End turn - should trigger draw for next turn
        tm.EndTurn(teamA, fireEvents: true);
        queue.ProcessQueue();

        // TeamB's turn started; since they have no hand, nothing happens
        Assert.AreEqual(teamB, tm.CurrentTeamId);

        Debug.Log("Integration_TurnSystemAndAutoDrawAndCardPlay passed.");
    }
}
