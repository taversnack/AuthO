<template>
<div v-if="showModal" class="modal-overlay">
  <div class="modal">
    <div class="modal-title">
      <h2>Printing & Returning Cards</h2>
    </div>
    <div class="modal-content">
      <ul>
        <li class="list-step">
          <font-awesome-icon icon="1" size="l"/>
          - On the home screen, press "I've forgotten my card"
        </li>
        <li class="list-step">
          <font-awesome-icon icon="2" size="l"/>
          - Enter your registered email address and select "Send me a code"
        </li>
        <ul class="pl-5">
          <li class="list-info">
            <font-awesome-icon icon="circle-info" size="l" class=""/>
            If the email address matches the one in our system, you will be sent a <bold class="font-semibold">One-Time</bold> Code via email 
          </li>
        </ul>  
        <li class="list-step">
          <font-awesome-icon icon="3" size="l"/>
          - Once you've recieved the code, select "I've got a code"
        </li>
        <li class="list-step">
          <font-awesome-icon icon="4" size="l"/>
          - Enter your code and select "Dispense Card"
        </li>
        <ul class="pl-5">
          <li class="list-info">
            <font-awesome-icon icon="circle-info" size="l"/>
            If the code is correct, you can take the dispensed temporary card below
          </li>
        </ul>
        <li class="list-step">
          <font-awesome-icon icon="5" size="l"/>
          - To return your temporary card, On the home screen, press "Return temporary card" 
        </li>
        <li class="list-step">
          <font-awesome-icon icon="6" size="l"/>
          - Insert card into the Kiosk below
        </li>
      </ul>
    </div>
    <button class="action-button" @click="close">Ok</button>
  </div>
</div>
</template>
  
<script lang="ts">
import { defineComponent } from 'vue';

export default defineComponent({
  props: {
    showModal: Boolean,
  },
  methods: {
    close() {
      this.$emit('close');
    },
  },
});
</script>

<style scoped>
.modal-overlay {
  @apply 
  flex 
  fixed 
  top-0 
  left-0 
  w-full 
  h-full 
  justify-center 
  items-center;

  background: rgba(0, 0, 0, 0.5);
}

.action-button {
  @apply 
bg-[#0180cc]
  px-5 py-3 
  rounded-3xl 
text-white
hover:bg-white
hover:text-gray
  font-semibold
  w-1/4
  text-2xl;

  border-width: 0.1em;
}

.modal {
  @apply 
bg-white 
  rounded-xl 
text-gray pb-5;

  max-width: 80%;
}

.modal-title {
  @apply 
  text-center 
  font-semibold 
  bg-[#0180cc] 
  text-white 
  text-2xl 
  border-b-2 
  pt-4 pb-4 
  rounded-t-xl;
}

.modal-content {
  @apply 
  text-black 
  text-lg 
  text-left 
  px-5 py-5;
}
  
.list-step {
  @apply 
  px-2 py-2 
  mb-2 
  text-white 
  bg-[#0180cc] 
  rounded-lg 
  font-semibold;

  border-width: 0.2em;
}

.list-info {
  @apply 
  px-1 py-1 
  mb-1;
}
</style>