<template>
  <div class="flex h-screen bg-gray-50 overflow-hidden">
    <Sidebar />
    <div class="flex-1 flex flex-col min-w-0 overflow-hidden">
      <TopBar />
      <main class="flex-1 overflow-auto p-4">
        <router-view />
      </main>
    </div>
  </div>
</template>

<script setup lang="ts">
import Sidebar from './Sidebar.vue'
import TopBar from './TopBar.vue'
</script>
