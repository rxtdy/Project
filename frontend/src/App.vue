<template>
  <v-app>
    <v-app-bar app color="primary" dark>
      <v-toolbar-title>Система управления ВУЗом</v-toolbar-title>
      <v-spacer></v-spacer>
      <v-btn text to="/" exact>Список групп</v-btn>
    </v-app-bar>

    <v-main class="bg-grey-lighten-4">
      <keep-alive include="GroupsTable">
        <router-view :key="$route.fullPath"></router-view>
      </keep-alive>
    </v-main>
  </v-app>
</template>

<script>
export default {
  name: 'App'
};
</script>