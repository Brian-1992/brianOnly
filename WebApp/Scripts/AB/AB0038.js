Ext.Loader.setConfig({
    enabled: true,
    paths: {
        'WEBAPP': '/Scripts/app'
    }
});

Ext.require(['WEBAPP.utils.Common']);


Ext.onReady(function () {
    Ext.override(Ext.grid.column.Column, { menuDisabled: true });

    // ＊＊＊＊＊＊＊＊＊＊＊＊＊＊＊＊ 參數 ＊＊＊＊＊＊＊＊＊＊＊＊＊＊＊＊
    {
        var T1Name = '比對到之資料';
        var T2Name = '比對訊息(資料檢核有誤)';
        var T3Name = '比對訊息(無異動)';
        var T4Name = '上傳檔案資料';
        var T5Name = '載入訊息';
        var T6Name = '比對資訊(查詢無資料)';

        var T1Rec = 0;
        var T1LastRec = null;
        var T2Rec = 0;
        var T2LastRec = null;
        var T3Rec = 0;
        var T3LastRec = null;
        var T4Rec = 0;
        var T4LastRec = null;
        var T5Rec = 0;
        var T5LastRec = null;
        var T6Rec = 0;
        var T6LastRec = null;

        var viewModel = Ext.create('WEBAPP.store.AB.AB0038');
    }
    // ＊＊＊＊＊＊＊＊＊＊＊＊＊＊＊＊＊ T1 ＊＊＊＊＊＊＊＊＊＊＊＊＊＊＊＊＊
    {
        // T1 Store
        var T1Store = viewModel.getStore('AB0038_1');

        // T1 Tool
        var T1Tool = Ext.create('Ext.PagingToolbar', {
            store: T1Store,
            displayInfo: true,
            border: false,
            plain: true,
            buttons: [
                {
                    xtype: 'filefield',
                    name: 'load_1',
                    id: 'load_1',
                    buttonOnly: true,
                    buttonText: '載入',
                    width: 40,
                    listeners: {
                        change: function (widget, value, eOpts) {
                            T1Store.removeAll();
                            T2Store.removeAll();
                            T3Store.removeAll();
                            T4Store.removeAll();
                            T5Store.removeAll();
                            T6Store.removeAll();
                            var files = event.target.files;
                            var self = this; // the controller
                            if (!files || files.length == 0) return; // make sure we got something
                            var f = files[0];
                            var ext = this.value.split('.').pop();
                            {
                                var myMask = new Ext.LoadMask(viewport, { msg: '處理中...' });
                                msglabel("訊息區: 檔案載入中......");
                                myMask.show();
                                var formData = new FormData();
                                formData.append("file", f);
                                var ajaxRequest = $.ajax({
                                    type: "POST",
                                    url: "/api/AB0038/LoadingCheck",
                                    data: formData,
                                    processData: false,
                                    //必須false才會自動加上正確的Content-Type
                                    contentType: false,
                                    success: function (data, textStatus, jqXHR) {
                                        if (!data.success) {
                                            T1Store.removeAll();
                                            T2Store.removeAll();
                                            T3Store.removeAll();
                                            T4Store.removeAll();
                                            T5Store.removeAll();
                                            T6Store.removeAll();
                                            Ext.MessageBox.alert("提示", data.msg);
                                            msglabel("訊息區: 檔案載入失敗");
                                            T5Grid.getStore().data.items[0].data.LOAD_MSG = "檔案載入失敗";
                                            T5Grid.getView().refresh();
                                            Ext.getCmp('commit_1').setDisabled(true);
                                            Ext.getCmp('commit_2').setDisabled(true);
                                            Ext.getCmp('commit_3').setDisabled(true);
                                            Ext.getCmp('commit_4').setDisabled(true);
                                            Ext.getCmp('commit_5').setDisabled(true);
                                            Ext.getCmp('commit_6').setDisabled(true);
                                        }
                                        else {
                                            msglabel("訊息區: " + data.etts[0].Grid_5[0].LOAD_MSG);
                                            T1Store.loadData(data.etts[0].Grid_1, false);
                                            T2Store.loadData(data.etts[0].Grid_2, false);
                                            T3Store.loadData(data.etts[0].Grid_3, false);
                                            T4Store.loadData(data.etts[0].Grid_4, false);
                                            T5Store.loadData(data.etts[0].Grid_5, false);
                                            T6Store.loadData(data.etts[0].Grid_6, false);
                                            if (data.etts[0].Grid_1.length != 0) {
                                                Ext.getCmp('commit_1').setDisabled(false);
                                                Ext.getCmp('commit_2').setDisabled(false);
                                                Ext.getCmp('commit_3').setDisabled(false);
                                                Ext.getCmp('commit_4').setDisabled(false);
                                                Ext.getCmp('commit_5').setDisabled(false);
                                                Ext.getCmp('commit_6').setDisabled(false);
                                            }
                                        }
                                        Ext.getCmp('load_1').fileInputEl.dom.value = '';
                                        myMask.hide();
                                    },
                                    error: function (jqXHR, textStatus, errorThrown) {
                                        Ext.Msg.alert('失敗', 'Ajax communication failed');
                                        T5Grid.getStore().data.items[0].data.LOAD_MSG = "檔案載入失敗";
                                        T5Grid.getView().refresh();
                                        Ext.getCmp('load_1').fileInputEl.dom.value = '';
                                        Ext.getCmp('commit_1').setDisabled(true);
                                        Ext.getCmp('commit_2').setDisabled(true);
                                        Ext.getCmp('commit_3').setDisabled(true);
                                        Ext.getCmp('commit_4').setDisabled(true);
                                        Ext.getCmp('commit_5').setDisabled(true);
                                        Ext.getCmp('commit_6').setDisabled(true);
                                        myMask.hide();
                                    }
                                });
                            }
                        }
                    }
                },
                {
                    id: 'commit_1', text: '儲存', disabled: true, handler: function () {
                        
                        var myMask = new Ext.LoadMask(viewport, { msg: '處理中...' });
                        myMask.show();
                        Ext.Ajax.request({
                            url: '/api/AB0038/Commit',
                            method: reqVal_p,
                            params: {
                                data: Ext.encode(Ext.pluck(T1Store.data.items, 'data'))
                            },
                            success: function (response) {
                                Ext.MessageBox.alert("提示", "儲存成功");
                                msglabel("訊息區:儲存成功");
                                Ext.getCmp('commit_1').setDisabled(true);
                                Ext.getCmp('commit_2').setDisabled(true);
                                Ext.getCmp('commit_3').setDisabled(true);
                                Ext.getCmp('commit_4').setDisabled(true);
                                Ext.getCmp('commit_5').setDisabled(true);
                                Ext.getCmp('commit_6').setDisabled(true);
                                myMask.hide();
                            },
                            failure: function (form, action) {
                                myMask.hide();
                                switch (action.failureType) {
                                    case Ext.form.action.Action.CLIENT_INVALID:
                                        Ext.Msg.alert('失敗', 'Form fields may not be submitted with invalid values');
                                        break;
                                    case Ext.form.action.Action.CONNECT_FAILURE:
                                        Ext.Msg.alert('失敗', 'Ajax communication failed');
                                        break;
                                    case Ext.form.action.Action.SERVER_INVALID:
                                        Ext.Msg.alert('失敗', "匯入失敗");
                                        break;
                                }
                            }
                        });
                    }
                },
                {
                    id: 'open_window_1', text: '欄位設定', handler: function () {
                        newApplyWindow.show();
                    }
                }
            ]
        });

        // T1 Grid
        var T1Grid = Ext.create('Ext.grid.Panel', {
            //title: T1Name,
            store: T1Store,
            plain: true,
            loadingText: '處理中...',
            loadMask: true,
            cls: 'T1',
            dockedItems: [{
                dock: 'top',
                xtype: 'toolbar',
                items: [T1Tool]
            }],
            listeners: {
                selectionchange: function (model, records) {
                    T1Rec = records.length;
                    T1LastRec = records[0];
                    if (T1LastRec) {
                        msglabel("");
                    }
                }
            }
        });
    }
    // ＊＊＊＊＊＊＊＊＊＊＊＊＊＊＊＊＊ T2 ＊＊＊＊＊＊＊＊＊＊＊＊＊＊＊＊＊
    {
        // T2 Store
        var T2Store = viewModel.getStore('AB0038_2');

        // T2 Tool
        var T2Tool = Ext.create('Ext.PagingToolbar', {
            store: T2Store,
            displayInfo: true,
            border: false,
            plain: true,
            buttons: [
                {
                    xtype: 'filefield',
                    name: 'load_2',
                    id: 'load_2',
                    buttonOnly: true,
                    buttonText: '載入',
                    width: 40,
                    listeners: {
                        change: function (widget, value, eOpts) {
                            T1Store.removeAll();
                            T2Store.removeAll();
                            T3Store.removeAll();
                            T4Store.removeAll();
                            T5Store.removeAll();
                            T6Store.removeAll();
                            var files = event.target.files;
                            var self = this; // the controller
                            if (!files || files.length == 0) return; // make sure we got something
                            var f = files[0];
                            var ext = this.value.split('.').pop();
                            {
                                var myMask = new Ext.LoadMask(viewport, { msg: '處理中...' });
                                msglabel("訊息區: 檔案載入中......");
                                myMask.show();
                                var formData = new FormData();
                                formData.append("file", f);
                                var ajaxRequest = $.ajax({
                                    type: "POST",
                                    url: "/api/AB0038/LoadingCheck",
                                    data: formData,
                                    processData: false,
                                    //必須false才會自動加上正確的Content-Type
                                    contentType: false,
                                    success: function (data, textStatus, jqXHR) {
                                        if (!data.success) {
                                            T1Store.removeAll();
                                            T2Store.removeAll();
                                            T3Store.removeAll();
                                            T4Store.removeAll();
                                            T5Store.removeAll();
                                            T6Store.removeAll();
                                            Ext.MessageBox.alert("提示", data.msg);
                                            msglabel("訊息區: 檔案載入失敗");
                                            T5Grid.getStore().data.items[0].data.LOAD_MSG = "檔案載入失敗";
                                            T5Grid.getView().refresh();
                                            Ext.getCmp('commit_1').setDisabled(true);
                                            Ext.getCmp('commit_2').setDisabled(true);
                                            Ext.getCmp('commit_3').setDisabled(true);
                                            Ext.getCmp('commit_4').setDisabled(true);
                                            Ext.getCmp('commit_5').setDisabled(true);
                                            Ext.getCmp('commit_6').setDisabled(true);
                                        }
                                        else {
                                            msglabel("訊息區: " + data.etts[0].Grid_5[0].LOAD_MSG);
                                            T1Store.loadData(data.etts[0].Grid_1, false);
                                            T2Store.loadData(data.etts[0].Grid_2, false);
                                            T3Store.loadData(data.etts[0].Grid_3, false);
                                            T4Store.loadData(data.etts[0].Grid_4, false);
                                            T5Store.loadData(data.etts[0].Grid_5, false);
                                            T6Store.loadData(data.etts[0].Grid_6, false);
                                            if (data.etts[0].Grid_1.length != 0) {
                                                Ext.getCmp('commit_1').setDisabled(false);
                                                Ext.getCmp('commit_2').setDisabled(false);
                                                Ext.getCmp('commit_3').setDisabled(false);
                                                Ext.getCmp('commit_4').setDisabled(false);
                                                Ext.getCmp('commit_5').setDisabled(false);
                                                Ext.getCmp('commit_6').setDisabled(false);
                                            }
                                        }
                                        Ext.getCmp('load_2').fileInputEl.dom.value = '';
                                        myMask.hide();
                                    },
                                    error: function (jqXHR, textStatus, errorThrown) {
                                        Ext.Msg.alert('失敗', 'Ajax communication failed');
                                        T5Grid.getStore().data.items[0].data.LOAD_MSG = "檔案載入失敗";
                                        T5Grid.getView().refresh();
                                        Ext.getCmp('load_2').fileInputEl.dom.value = '';
                                        Ext.getCmp('commit_1').setDisabled(true);
                                        Ext.getCmp('commit_2').setDisabled(true);
                                        Ext.getCmp('commit_3').setDisabled(true);
                                        Ext.getCmp('commit_4').setDisabled(true);
                                        Ext.getCmp('commit_5').setDisabled(true);
                                        Ext.getCmp('commit_6').setDisabled(true);
                                        myMask.hide();
                                    }
                                });
                            }
                        }
                    }
                },
                {
                    id: 'commit_2', text: '儲存', disabled: true, handler: function () {
                        var myMask = new Ext.LoadMask(viewport, { msg: '處理中...' });
                        myMask.show();
                        Ext.Ajax.request({
                            url: '/api/AB0038/Commit',
                            method: reqVal_p,
                            params: {
                                data: Ext.encode(Ext.pluck(T1Store.data.items, 'data'))
                            },
                            success: function (response) {
                                Ext.MessageBox.alert("提示", "儲存成功");
                                msglabel("訊息區:儲存成功");
                                Ext.getCmp('commit_1').setDisabled(true);
                                Ext.getCmp('commit_2').setDisabled(true);
                                Ext.getCmp('commit_3').setDisabled(true);
                                Ext.getCmp('commit_4').setDisabled(true);
                                Ext.getCmp('commit_5').setDisabled(true);
                                Ext.getCmp('commit_6').setDisabled(true);
                                myMask.hide();
                            },
                            failure: function (form, action) {
                                myMask.hide();
                                switch (action.failureType) {
                                    case Ext.form.action.Action.CLIENT_INVALID:
                                        Ext.Msg.alert('失敗', 'Form fields may not be submitted with invalid values');
                                        break;
                                    case Ext.form.action.Action.CONNECT_FAILURE:
                                        Ext.Msg.alert('失敗', 'Ajax communication failed');
                                        break;
                                    case Ext.form.action.Action.SERVER_INVALID:
                                        Ext.Msg.alert('失敗', "匯入失敗");
                                        break;
                                }
                            }
                        });
                    }
                },
                {
                    id: 'open_window_2', text: '欄位設定', handler: function () {
                        newApplyWindow.show();
                    }
                }
            ]
        });

        // T2 Grid
        var T2Grid = Ext.create('Ext.grid.Panel', {
            //title: T1Name,
            store: T2Store,
            plain: true,
            loadingText: '處理中...',
            loadMask: true,
            cls: 'T1',
            dockedItems: [{
                dock: 'top',
                xtype: 'toolbar',
                items: [T2Tool]
            }],
            listeners: {
                selectionchange: function (model, records) {
                    T2Rec = records.length;
                    T2LastRec = records[0];
                    if (T2LastRec) {
                        msglabel("");
                    }
                }
            }
        });

    }
    // ＊＊＊＊＊＊＊＊＊＊＊＊＊＊＊＊＊ T3 ＊＊＊＊＊＊＊＊＊＊＊＊＊＊＊＊＊
    {
        // T3 Store
        var T3Store = viewModel.getStore('AB0038_3');

        // T3 Tool
        var T3Tool = Ext.create('Ext.PagingToolbar', {
            store: T3Store,
            displayInfo: true,
            border: false,
            plain: true,
            buttons: [
                {
                    xtype: 'filefield',
                    name: 'load_3',
                    id: 'load_3',
                    buttonOnly: true,
                    buttonText: '載入',
                    width: 40,
                    listeners: {
                        change: function (widget, value, eOpts) {
                            T1Store.removeAll();
                            T2Store.removeAll();
                            T3Store.removeAll();
                            T4Store.removeAll();
                            T5Store.removeAll();
                            T6Store.removeAll();
                            var files = event.target.files;
                            var self = this; // the controller
                            if (!files || files.length == 0) return; // make sure we got something
                            var f = files[0];
                            var ext = this.value.split('.').pop();
                            {
                                var myMask = new Ext.LoadMask(viewport, { msg: '處理中...' });
                                msglabel("訊息區: 檔案載入中......");
                                myMask.show();
                                var formData = new FormData();
                                formData.append("file", f);
                                var ajaxRequest = $.ajax({
                                    type: "POST",
                                    url: "/api/AB0038/LoadingCheck",
                                    data: formData,
                                    processData: false,
                                    //必須false才會自動加上正確的Content-Type
                                    contentType: false,
                                    success: function (data, textStatus, jqXHR) {
                                        if (!data.success) {
                                            T1Store.removeAll();
                                            T2Store.removeAll();
                                            T3Store.removeAll();
                                            T4Store.removeAll();
                                            T5Store.removeAll();
                                            T6Store.removeAll();
                                            Ext.MessageBox.alert("提示", data.msg);
                                            msglabel("訊息區: 檔案載入失敗");
                                            T5Grid.getStore().data.items[0].data.LOAD_MSG = "檔案載入失敗";
                                            T5Grid.getView().refresh();
                                            Ext.getCmp('commit_1').setDisabled(true);
                                            Ext.getCmp('commit_2').setDisabled(true);
                                            Ext.getCmp('commit_3').setDisabled(true);
                                            Ext.getCmp('commit_4').setDisabled(true);
                                            Ext.getCmp('commit_5').setDisabled(true);
                                            Ext.getCmp('commit_6').setDisabled(true);
                                        }
                                        else {
                                            msglabel("訊息區: " + data.etts[0].Grid_5[0].LOAD_MSG);
                                            T1Store.loadData(data.etts[0].Grid_1, false);
                                            T2Store.loadData(data.etts[0].Grid_2, false);
                                            T3Store.loadData(data.etts[0].Grid_3, false);
                                            T4Store.loadData(data.etts[0].Grid_4, false);
                                            T5Store.loadData(data.etts[0].Grid_5, false);
                                            T6Store.loadData(data.etts[0].Grid_6, false);
                                            if (data.etts[0].Grid_1.length != 0) {
                                                Ext.getCmp('commit_1').setDisabled(false);
                                                Ext.getCmp('commit_2').setDisabled(false);
                                                Ext.getCmp('commit_3').setDisabled(false);
                                                Ext.getCmp('commit_4').setDisabled(false);
                                                Ext.getCmp('commit_5').setDisabled(false);
                                                Ext.getCmp('commit_6').setDisabled(false);
                                            }
                                        }
                                        Ext.getCmp('load_3').fileInputEl.dom.value = '';
                                        myMask.hide();
                                    },
                                    error: function (jqXHR, textStatus, errorThrown) {
                                        Ext.Msg.alert('失敗', 'Ajax communication failed');
                                        T5Grid.getStore().data.items[0].data.LOAD_MSG = "檔案載入中失敗";
                                        T5Grid.getView().refresh();
                                        Ext.getCmp('load_3').fileInputEl.dom.value = '';
                                        Ext.getCmp('commit_1').setDisabled(true);
                                        Ext.getCmp('commit_2').setDisabled(true);
                                        Ext.getCmp('commit_3').setDisabled(true);
                                        Ext.getCmp('commit_4').setDisabled(true);
                                        Ext.getCmp('commit_5').setDisabled(true);
                                        Ext.getCmp('commit_6').setDisabled(true);
                                        myMask.hide();
                                    }
                                });
                            }
                        }
                    }
                },
                {
                    id: 'commit_3', text: '儲存', disabled: true, handler: function () {
                        var myMask = new Ext.LoadMask(viewport, { msg: '處理中...' });
                        myMask.show();
                        Ext.Ajax.request({
                            url: '/api/AB0038/Commit',
                            method: reqVal_p,
                            params: {
                                data: Ext.encode(Ext.pluck(T1Store.data.items, 'data'))
                            },
                            success: function (response) {
                                Ext.MessageBox.alert("提示", "儲存成功");
                                msglabel("訊息區:儲存成功");
                                Ext.getCmp('commit_1').setDisabled(true);
                                Ext.getCmp('commit_2').setDisabled(true);
                                Ext.getCmp('commit_3').setDisabled(true);
                                Ext.getCmp('commit_4').setDisabled(true);
                                Ext.getCmp('commit_5').setDisabled(true);
                                Ext.getCmp('commit_6').setDisabled(true);
                                myMask.hide();
                            },
                            failure: function (form, action) {
                                myMask.hide();
                                switch (action.failureType) {
                                    case Ext.form.action.Action.CLIENT_INVALID:
                                        Ext.Msg.alert('失敗', 'Form fields may not be submitted with invalid values');
                                        break;
                                    case Ext.form.action.Action.CONNECT_FAILURE:
                                        Ext.Msg.alert('失敗', 'Ajax communication failed');
                                        break;
                                    case Ext.form.action.Action.SERVER_INVALID:
                                        Ext.Msg.alert('失敗', "匯入失敗");
                                        break;
                                }
                            }
                        });
                    }
                },
                {
                    id: 'open_window_3', text: '欄位設定', handler: function () {
                        newApplyWindow.show();
                    }
                }
            ]
        });

        // T3 Grid
        var T3Grid = Ext.create('Ext.grid.Panel', {
            //title: T1Name,
            store: T3Store,
            plain: true,
            loadingText: '處理中...',
            loadMask: true,
            cls: 'T1',
            dockedItems: [{
                dock: 'top',
                xtype: 'toolbar',
                items: [T3Tool]
            }],
            listeners: {
                selectionchange: function (model, records) {
                    T3Rec = records.length;
                    T3LastRec = records[0];
                    if (T3LastRec) {
                        msglabel("");
                    }
                }
            }
        });

    }
    // ＊＊＊＊＊＊＊＊＊＊＊＊＊＊＊＊＊ T4 ＊＊＊＊＊＊＊＊＊＊＊＊＊＊＊＊＊
    {
        // T4 Store
        var T4Store = viewModel.getStore('AB0038_4');

        // T4 Tool
        var T4Tool = Ext.create('Ext.PagingToolbar', {
            store: T4Store,
            displayInfo: true,
            border: false,
            plain: true,
            buttons: [
                {
                    xtype: 'filefield',
                    name: 'load_4',
                    id: 'load_4',
                    buttonOnly: true,
                    buttonText: '載入',
                    width: 40,
                    listeners: {
                        change: function (widget, value, eOpts) {
                            T1Store.removeAll();
                            T2Store.removeAll();
                            T3Store.removeAll();
                            T4Store.removeAll();
                            T5Store.removeAll();
                            T6Store.removeAll();
                            var files = event.target.files;
                            var self = this; // the controller
                            if (!files || files.length == 0) return; // make sure we got something
                            var f = files[0];
                            var ext = this.value.split('.').pop();
                            {
                                var myMask = new Ext.LoadMask(viewport, { msg: '處理中...' });
                                msglabel("訊息區: 檔案載入中......");
                                myMask.show();
                                var formData = new FormData();
                                formData.append("file", f);
                                var ajaxRequest = $.ajax({
                                    type: "POST",
                                    url: "/api/AB0038/LoadingCheck",
                                    data: formData,
                                    processData: false,
                                    //必須false才會自動加上正確的Content-Type
                                    contentType: false,
                                    success: function (data, textStatus, jqXHR) {
                                        if (!data.success) {
                                            T1Store.removeAll();
                                            T2Store.removeAll();
                                            T3Store.removeAll();
                                            T4Store.removeAll();
                                            T5Store.removeAll();
                                            T6Store.removeAll();
                                            Ext.MessageBox.alert("提示", data.msg);
                                            msglabel("訊息區: 檔案載入失敗");
                                            T5Grid.getStore().data.items[0].data.LOAD_MSG = "檔案載入失敗";
                                            T5Grid.getView().refresh();
                                            Ext.getCmp('commit_1').setDisabled(true);
                                            Ext.getCmp('commit_2').setDisabled(true);
                                            Ext.getCmp('commit_3').setDisabled(true);
                                            Ext.getCmp('commit_4').setDisabled(true);
                                            Ext.getCmp('commit_5').setDisabled(true);
                                            Ext.getCmp('commit_6').setDisabled(true);
                                        }
                                        else {
                                            msglabel("訊息區: " + data.etts[0].Grid_5[0].LOAD_MSG);
                                            T1Store.loadData(data.etts[0].Grid_1, false);
                                            T2Store.loadData(data.etts[0].Grid_2, false);
                                            T3Store.loadData(data.etts[0].Grid_3, false);
                                            T4Store.loadData(data.etts[0].Grid_4, false);
                                            T5Store.loadData(data.etts[0].Grid_5, false);
                                            T6Store.loadData(data.etts[0].Grid_6, false);
                                            if (data.etts[0].Grid_1.length != 0) {
                                                Ext.getCmp('commit_1').setDisabled(false);
                                                Ext.getCmp('commit_2').setDisabled(false);
                                                Ext.getCmp('commit_3').setDisabled(false);
                                                Ext.getCmp('commit_4').setDisabled(false);
                                                Ext.getCmp('commit_5').setDisabled(false);
                                                Ext.getCmp('commit_6').setDisabled(false);
                                            }
                                        }
                                        Ext.getCmp('load_4').fileInputEl.dom.value = '';
                                        myMask.hide();
                                    },
                                    error: function (jqXHR, textStatus, errorThrown) {
                                        Ext.Msg.alert('失敗', 'Ajax communication failed');
                                        T5Grid.getStore().data.items[0].data.LOAD_MSG = "檔案載入失敗";
                                        T5Grid.getView().refresh();
                                        Ext.getCmp('load_4').fileInputEl.dom.value = '';
                                        Ext.getCmp('commit_1').setDisabled(true);
                                        Ext.getCmp('commit_2').setDisabled(true);
                                        Ext.getCmp('commit_3').setDisabled(true);
                                        Ext.getCmp('commit_4').setDisabled(true);
                                        Ext.getCmp('commit_5').setDisabled(true);
                                        Ext.getCmp('commit_6').setDisabled(true);
                                        myMask.hide();
                                    }
                                });
                            }
                        }
                    }
                },
                {
                    id: 'commit_4', text: '儲存', disabled: true, handler: function () {
                        var myMask = new Ext.LoadMask(viewport, { msg: '處理中...' });
                        myMask.show();
                        Ext.Ajax.request({
                            url: '/api/AB0038/Commit',
                            method: reqVal_p,
                            params: {
                                data: Ext.encode(Ext.pluck(T1Store.data.items, 'data'))
                            },
                            success: function (response) {
                                Ext.MessageBox.alert("提示", "儲存成功");
                                msglabel("訊息區:儲存成功");
                                Ext.getCmp('commit_1').setDisabled(true);
                                Ext.getCmp('commit_2').setDisabled(true);
                                Ext.getCmp('commit_3').setDisabled(true);
                                Ext.getCmp('commit_4').setDisabled(true);
                                Ext.getCmp('commit_5').setDisabled(true);
                                Ext.getCmp('commit_6').setDisabled(true);
                                myMask.hide();
                            },
                            failure: function (form, action) {
                                myMask.hide();
                                switch (action.failureType) {
                                    case Ext.form.action.Action.CLIENT_INVALID:
                                        Ext.Msg.alert('失敗', 'Form fields may not be submitted with invalid values');
                                        break;
                                    case Ext.form.action.Action.CONNECT_FAILURE:
                                        Ext.Msg.alert('失敗', 'Ajax communication failed');
                                        break;
                                    case Ext.form.action.Action.SERVER_INVALID:
                                        Ext.Msg.alert('失敗', "匯入失敗");
                                        break;
                                }
                            }
                        });
                    }
                },
                {
                    id: 'open_window_4', text: '欄位設定', handler: function () {
                        newApplyWindow.show();
                    }
                }
            ]
        });

        // T4 Grid
        var T4Grid = Ext.create('Ext.grid.Panel', {
            //title: T1Name,
            store: T4Store,
            plain: true,
            loadingText: '處理中...',
            loadMask: true,
            cls: 'T1',
            dockedItems: [{
                dock: 'top',
                xtype: 'toolbar',
                items: [T4Tool]
            }],
            listeners: {
                selectionchange: function (model, records) {
                    T4Rec = records.length;
                    T4LastRec = records[0];
                    if (T4LastRec) {
                        msglabel("");
                    }
                }
            }
        });

    }
    // ＊＊＊＊＊＊＊＊＊＊＊＊＊＊＊＊＊ T5 ＊＊＊＊＊＊＊＊＊＊＊＊＊＊＊＊＊
    {
        // T5 Store
        var T5Store = viewModel.getStore('AB0038_5');

        // T5 Tool
        var T5Tool = Ext.create('Ext.PagingToolbar', {
            store: T5Store,
            displayInfo: true,
            border: false,
            plain: true,
            buttons: [
                {
                    xtype: 'filefield',
                    name: 'load_5',
                    id: 'load_5',
                    buttonOnly: true,
                    buttonText: '載入',
                    width: 40,
                    listeners: {
                        change: function (widget, value, eOpts) {
                            T1Store.removeAll();
                            T2Store.removeAll();
                            T3Store.removeAll();
                            T4Store.removeAll();
                            T5Store.removeAll();
                            T6Store.removeAll();
                            var files = event.target.files;
                            var self = this; // the controller
                            if (!files || files.length == 0) return; // make sure we got something
                            var f = files[0];
                            var ext = this.value.split('.').pop();
                            {
                                var myMask = new Ext.LoadMask(viewport, { msg: '處理中...' });
                                msglabel("訊息區: 檔案載入中......");
                                myMask.show();
                                var formData = new FormData();
                                formData.append("file", f);
                                var ajaxRequest = $.ajax({
                                    type: "POST",
                                    url: "/api/AB0038/LoadingCheck",
                                    data: formData,
                                    processData: false,
                                    //必須false才會自動加上正確的Content-Type
                                    contentType: false,
                                    success: function (data, textStatus, jqXHR) {
                                        if (!data.success) {
                                            T1Store.removeAll();
                                            T2Store.removeAll();
                                            T3Store.removeAll();
                                            T4Store.removeAll();
                                            T5Store.removeAll();
                                            T6Store.removeAll();
                                            Ext.MessageBox.alert("提示", data.msg);
                                            msglabel("訊息區: 檔案載入失敗");
                                            T5Grid.getStore().data.items[0].data.LOAD_MSG = "檔案載入失敗";
                                            T5Grid.getView().refresh();
                                            Ext.getCmp('commit_1').setDisabled(true);
                                            Ext.getCmp('commit_2').setDisabled(true);
                                            Ext.getCmp('commit_3').setDisabled(true);
                                            Ext.getCmp('commit_4').setDisabled(true);
                                            Ext.getCmp('commit_5').setDisabled(true);
                                            Ext.getCmp('commit_6').setDisabled(true);
                                        }
                                        else {
                                            msglabel("訊息區: " + data.etts[0].Grid_5[0].LOAD_MSG);
                                            T1Store.loadData(data.etts[0].Grid_1, false);
                                            T2Store.loadData(data.etts[0].Grid_2, false);
                                            T3Store.loadData(data.etts[0].Grid_3, false);
                                            T4Store.loadData(data.etts[0].Grid_4, false);
                                            T5Store.loadData(data.etts[0].Grid_5, false);
                                            T6Store.loadData(data.etts[0].Grid_6, false);
                                            if (data.etts[0].Grid_1.length != 0) {
                                                Ext.getCmp('commit_1').setDisabled(false);
                                                Ext.getCmp('commit_2').setDisabled(false);
                                                Ext.getCmp('commit_3').setDisabled(false);
                                                Ext.getCmp('commit_4').setDisabled(false);
                                                Ext.getCmp('commit_5').setDisabled(false);
                                                Ext.getCmp('commit_6').setDisabled(false);
                                            }
                                        }
                                        Ext.getCmp('load_5').fileInputEl.dom.value = '';
                                        myMask.hide();
                                    },
                                    error: function (jqXHR, textStatus, errorThrown) {
                                        Ext.Msg.alert('失敗', 'Ajax communication failed');
                                        T5Grid.getStore().data.items[0].data.LOAD_MSG = "檔案載入失敗";
                                        T5Grid.getView().refresh();
                                        Ext.getCmp('load_5').fileInputEl.dom.value = '';
                                        Ext.getCmp('commit_1').setDisabled(true);
                                        Ext.getCmp('commit_2').setDisabled(true);
                                        Ext.getCmp('commit_3').setDisabled(true);
                                        Ext.getCmp('commit_4').setDisabled(true);
                                        Ext.getCmp('commit_5').setDisabled(true);
                                        Ext.getCmp('commit_6').setDisabled(true);
                                        myMask.hide();
                                    }
                                });
                            }
                        }
                    }
                },
                {
                    id: 'commit_5', text: '儲存', disabled: true, handler: function () {
                        var myMask = new Ext.LoadMask(viewport, { msg: '處理中...' });
                        myMask.show();
                        Ext.Ajax.request({
                            url: '/api/AB0038/Commit',
                            method: reqVal_p,
                            params: {
                                data: Ext.encode(Ext.pluck(T1Store.data.items, 'data'))
                            },
                            success: function (response) {
                                Ext.MessageBox.alert("提示", "儲存成功");
                                msglabel("訊息區:儲存成功");
                                Ext.getCmp('commit_1').setDisabled(true);
                                Ext.getCmp('commit_2').setDisabled(true);
                                Ext.getCmp('commit_3').setDisabled(true);
                                Ext.getCmp('commit_4').setDisabled(true);
                                Ext.getCmp('commit_5').setDisabled(true);
                                Ext.getCmp('commit_6').setDisabled(true);
                                myMask.hide();
                            },
                            failure: function (form, action) {
                                myMask.hide();
                                switch (action.failureType) {
                                    case Ext.form.action.Action.CLIENT_INVALID:
                                        Ext.Msg.alert('失敗', 'Form fields may not be submitted with invalid values');
                                        break;
                                    case Ext.form.action.Action.CONNECT_FAILURE:
                                        Ext.Msg.alert('失敗', 'Ajax communication failed');
                                        break;
                                    case Ext.form.action.Action.SERVER_INVALID:
                                        Ext.Msg.alert('失敗', "匯入失敗");
                                        break;
                                }
                            }
                        });
                    }
                },
                {
                    id: 'open_window_5', text: '欄位設定', handler: function () {
                        newApplyWindow.show();
                    }
                }
            ]
        });

        // T5 Grid
        var T5Grid = Ext.create('Ext.grid.Panel', {
            //title: T1Name,
            store: T5Store,
            plain: true,
            loadingText: '處理中...',
            loadMask: true,
            cls: 'T1',
            dockedItems: [{
                dock: 'top',
                xtype: 'toolbar',
                items: [T5Tool]
            }],
            listeners: {
                selectionchange: function (model, records) {
                    T5Rec = records.length;
                    T5LastRec = records[0];
                    if (T5LastRec) {
                        msglabel("");
                    }
                }
            }
        });

    }
    // ＊＊＊＊＊＊＊＊＊＊＊＊＊＊＊＊＊ T6 ＊＊＊＊＊＊＊＊＊＊＊＊＊＊＊＊＊
    {
        // T6 Store
        var T6Store = viewModel.getStore('AB0038_6');

        // T6 Tool
        var T6Tool = Ext.create('Ext.PagingToolbar', {
            store: T6Store,
            displayInfo: true,
            border: false,
            plain: true,
            buttons: [
                {
                    xtype: 'filefield',
                    name: 'load_6',
                    id: 'load_6',
                    buttonOnly: true,
                    buttonText: '載入',
                    width: 40,
                    listeners: {
                        change: function (widget, value, eOpts) {
                            T1Store.removeAll();
                            T2Store.removeAll();
                            T3Store.removeAll();
                            T4Store.removeAll();
                            T5Store.removeAll();
                            T6Store.removeAll();
                            var files = event.target.files;
                            var self = this; // the controller
                            if (!files || files.length == 0) return; // make sure we got something
                            var f = files[0];
                            var ext = this.value.split('.').pop();
                            {
                                var myMask = new Ext.LoadMask(viewport, { msg: '處理中...' });
                                msglabel("訊息區: 檔案載入中......");
                                myMask.show();
                                var formData = new FormData();
                                formData.append("file", f);
                                var ajaxRequest = $.ajax({
                                    type: "POST",
                                    url: "/api/AB0038/LoadingCheck",
                                    data: formData,
                                    processData: false,
                                    //必須false才會自動加上正確的Content-Type
                                    contentType: false,
                                    success: function (data, textStatus, jqXHR) {
                                        if (!data.success) {
                                            T1Store.removeAll();
                                            T2Store.removeAll();
                                            T3Store.removeAll();
                                            T4Store.removeAll();
                                            T5Store.removeAll();
                                            T6Store.removeAll();
                                            Ext.MessageBox.alert("提示", data.msg);
                                            msglabel("訊息區: 檔案載入失敗");
                                            T5Grid.getStore().data.items[0].data.LOAD_MSG = "檔案載入失敗";
                                            T5Grid.getView().refresh();
                                            Ext.getCmp('commit_1').setDisabled(true);
                                            Ext.getCmp('commit_2').setDisabled(true);
                                            Ext.getCmp('commit_3').setDisabled(true);
                                            Ext.getCmp('commit_4').setDisabled(true);
                                            Ext.getCmp('commit_5').setDisabled(true);
                                            Ext.getCmp('commit_6').setDisabled(true);
                                        }
                                        else {
                                            msglabel("訊息區: " + data.etts[0].Grid_5[0].LOAD_MSG);
                                            T1Store.loadData(data.etts[0].Grid_1, false);
                                            T2Store.loadData(data.etts[0].Grid_2, false);
                                            T3Store.loadData(data.etts[0].Grid_3, false);
                                            T4Store.loadData(data.etts[0].Grid_4, false);
                                            T5Store.loadData(data.etts[0].Grid_5, false);
                                            T6Store.loadData(data.etts[0].Grid_6, false);
                                            if (data.etts[0].Grid_1.length != 0) {
                                                Ext.getCmp('commit_1').setDisabled(false);
                                                Ext.getCmp('commit_2').setDisabled(false);
                                                Ext.getCmp('commit_3').setDisabled(false);
                                                Ext.getCmp('commit_4').setDisabled(false);
                                                Ext.getCmp('commit_5').setDisabled(false);
                                                Ext.getCmp('commit_6').setDisabled(false);
                                            }
                                        }
                                        Ext.getCmp('load_6').fileInputEl.dom.value = '';
                                        myMask.hide();
                                    },
                                    error: function (jqXHR, textStatus, errorThrown) {
                                        Ext.Msg.alert('失敗', 'Ajax communication failed');
                                        T5Grid.getStore().data.items[0].data.LOAD_MSG = "檔案載入失敗";
                                        T5Grid.getView().refresh();
                                        Ext.getCmp('load_6').fileInputEl.dom.value = '';
                                        Ext.getCmp('commit_1').setDisabled(true);
                                        Ext.getCmp('commit_2').setDisabled(true);
                                        Ext.getCmp('commit_3').setDisabled(true);
                                        Ext.getCmp('commit_4').setDisabled(true);
                                        Ext.getCmp('commit_5').setDisabled(true);
                                        Ext.getCmp('commit_6').setDisabled(true);
                                        myMask.hide();
                                    }
                                });
                            }
                        }
                    }
                },
                {
                    id: 'commit_6', text: '儲存', disabled: true, handler: function () {
                        var myMask = new Ext.LoadMask(viewport, { msg: '處理中...' });
                        myMask.show();
                        Ext.Ajax.request({
                            url: '/api/AB0038/Commit',
                            method: reqVal_p,
                            params: {
                                data: Ext.encode(Ext.pluck(T1Store.data.items, 'data'))
                            },
                            success: function (response) {
                                Ext.MessageBox.alert("提示", "儲存成功");
                                msglabel("訊息區:儲存成功");
                                Ext.getCmp('commit_1').setDisabled(true);
                                Ext.getCmp('commit_2').setDisabled(true);
                                Ext.getCmp('commit_3').setDisabled(true);
                                Ext.getCmp('commit_4').setDisabled(true);
                                Ext.getCmp('commit_5').setDisabled(true);
                                Ext.getCmp('commit_6').setDisabled(true);
                                myMask.hide();
                            },
                            failure: function (form, action) {
                                myMask.hide();
                                switch (action.failureType) {
                                    case Ext.form.action.Action.CLIENT_INVALID:
                                        Ext.Msg.alert('失敗', 'Form fields may not be submitted with invalid values');
                                        break;
                                    case Ext.form.action.Action.CONNECT_FAILURE:
                                        Ext.Msg.alert('失敗', 'Ajax communication failed');
                                        break;
                                    case Ext.form.action.Action.SERVER_INVALID:
                                        Ext.Msg.alert('失敗', "匯入失敗");
                                        break;
                                }
                            }
                        });
                    }
                },
                {
                    id: 'open_window_6', text: '欄位設定', handler: function () {
                        newApplyWindow.show();
                    }
                }
            ]
        });

        // T6 Grid
        var T6Grid = Ext.create('Ext.grid.Panel', {
            //title: T1Name,
            store: T6Store,
            plain: true,
            loadingText: '處理中...',
            loadMask: true,
            cls: 'T1',
            dockedItems: [{
                dock: 'top',
                xtype: 'toolbar',
                items: [T6Tool]
            }],
            listeners: {
                selectionchange: function (model, records) {
                    T6Rec = records.length;
                    T6LastRec = records[0];
                    if (T6LastRec) {
                        msglabel("");
                    }
                }
            }
        });

    }
    //＊＊＊＊＊＊＊＊＊＊＊＊＊＊＊ 定義TAB內容 ＊＊＊＊＊＊＊＊＊＊＊＊＊＊＊
    {
        //設定各分頁
        var TATabs = Ext.widget('tabpanel', {
            listeners: {
                tabchange: function (tabpanel, newCard, oldCard) {
                    switch (newCard.title) {
                        case "比對到之資料":
                            //T1Form.setVisible(true);
                            // T2Form.setVisible(false);
                            //T1Query.getForm().findField('P0').focus();
                            break;
                        case "效期總表":
                            // T1Form.setVisible(false);
                            // T2Form.setVisible(true);
                            // T2Query.getForm().findField('P2').focus();
                            //T1Query.getForm().findField('P0').clearInvalid();
                            break;
                    }
                }
            },
            layout: 'fit',
            plain: true,
            border: false,
            resizeTabs: true,       //改變tab尺寸       
            enableTabScroll: true,  //是否允許Tab溢出時可以滾動
            defaults: {
                // autoScroll: true,
                closabel: false,    //tab是否可關閉
                padding: 0,
                split: true
            },
            items: [{
                itemId: 't1Grid',
                title: '比對到之資料',
                layout: 'border',
                padding: 0,
                split: true,
                region: 'center',
                layout: 'fit',
                collapsible: false,
                border: false,
                items: [T1Grid]
            }, {
                itemId: 't2Grid',
                title: '比對訊息(資料檢核有誤)',
                layout: 'border',
                padding: 0,
                split: true,
                region: 'center',
                layout: 'fit',
                collapsible: false,
                border: false,
                items: [T2Grid]
            }, {
                itemId: 't3Grid',
                title: '比對訊息(無異動)',
                layout: 'border',
                padding: 0,
                split: true,
                region: 'center',
                layout: 'fit',
                collapsible: false,
                border: false,
                items: [T3Grid]
            }, {
                itemId: 't4Grid',
                title: '上傳檔案資料',
                layout: 'border',
                padding: 0,
                split: true,
                region: 'center',
                layout: 'fit',
                collapsible: false,
                border: false,
                items: [T4Grid]
            }, {
                itemId: 't5Grid',
                title: '載入訊息',
                layout: 'border',
                padding: 0,
                split: true,
                region: 'center',
                layout: 'fit',
                collapsible: false,
                border: false,
                items: [T5Grid]
            }, {
                itemId: 't6Grid',
                title: '比對資訊(查詢無資料)',
                layout: 'border',
                padding: 0,
                split: true,
                region: 'center',
                layout: 'fit',
                collapsible: false,
                border: false,
                items: [T6Grid]
            }]
        });

        //設定viewport
        var viewport = Ext.create('Ext.Viewport', {
            renderTo: body,
            layout: {
                type: 'border',
                padding: 0
            },
            defaults: {
                split: true
            },
            items: [{
                itemId: 't1Form',
                region: 'center',
                layout: 'fit',
                collapsible: false,
                title: '',
                border: false,
                items: [TATabs]
            }
            ]
        });
    }
    //＊＊＊＊＊＊＊＊＊＊＊＊＊＊ 欄位設定彈出視窗 ＊＊＊＊＊＊＊＊＊＊＊＊＊＊
    {
        var windowWidth = $(window).width();
        var windowHeight = $(window).height();
        var T7LLastRec = null;
        var T7RLastRec = null;
        var ActionFlag = '';

        var CreateTempUrl = '/api/AB0038/CreateTemp';
        var DeleteTempUrl = '/api/AB0038/DeleteTemp';
        var ColSubmitUrl = '';

        var viewModel_VM = Ext.create('WEBAPP.store.AB.AB0038VM');

        //一進入程式，將TEMP檔清空
        function DeleteTemp() {
            Ext.Ajax.request({
                url: DeleteTempUrl,
                method: reqVal_p,
                success: function (response) {
                    CreateTemp();
                },
                failure: function (response, options) {

                }
            });
        }

        //一進入程式，將預設欄位匯入TEMP檔，並QUERY
        function CreateTemp() {
            Ext.Ajax.request({
                url: CreateTempUrl,
                method: reqVal_p,
                success: function (response) {
                    T7LLoad();
                    T7RLoad();
                },
                failure: function (response, options) {

                }
            });
        }

        //左視窗Store (基本欄位選單)
        var T7LStore = viewModel_VM.getStore('AB0038VM_L');

        //左視窗Tool(基本欄位選單)
        var T7LTool = Ext.create('Ext.PagingToolbar', {
            store: T7LStore,
            displayInfo: true,
            border: false,
            plain: true
        });

        //左視窗Query(基本欄位選單)
        var T7LQuery = Ext.widget({
            xtype: 'form',
            layout: 'form',
            border: false,
            autoScroll: true, // 若為true,容器內容超出容器大小時出現捲動條;若為false超出容器的部分會被裁切掉
            fieldDefaults: {
                xtype: 'textfield',
                labelAlign: 'right'
            },
            items: [{
                xtype: 'panel',
                id: 'PanelP7L',
                border: false,
                layout: 'hbox',
                items: [{
                    xtype: 'textfield',
                    fieldLabel: '',
                    labelSeparator: '',
                    name: 'P0',
                    labelWidth: 5,
                    width: 170//,
                    // padding: '0 2 0 0'
                }, {
                    xtype: 'button',
                    text: '查詢',
                    handler: function () {
                        msglabel("");
                        T7LLoad();
                    }
                }, {
                    xtype: 'button',
                    text: '清除',
                    handler: function () {
                        var f = this.up('form').getForm();
                        msglabel("");
                        f.reset();
                        f.findField('P0').setValue('');
                        f.findField('P0').focus(); // 進入畫面時輸入游標預設在P0
                    }
                }
                ]
            }]
        });

        //左視窗Grid (基本欄位選單)
        var T7LGrid = Ext.create('Ext.grid.Panel', {
            //title: T1Name,
            store: T7LStore,
            plain: true,
            loadingText: '處理中...',
            loadMask: true,
            cls: 'T1',
            dockedItems: [{
                dock: 'top',
                xtype: 'toolbar',
                layout: 'fit',
                items: [T7LQuery]
            }],
            columns: [{
                text: "順序",
                dataIndex: 'SEQ',
                hidden: true
            },
            {
                text: "中文名稱",
                dataIndex: 'CNAME',
                width: 220
            }, {
                text: "英文名稱",
                dataIndex: 'ENAME',
                width: 250
            },
            {
                header: "",
                flex: 1
            }],
            viewConfig: {
                listeners: {
                    refresh: function (view) {
                        T7RGrid.down('#JoinCol').setDisabled(true);
                    }
                }
            },
            listeners: {
                itemclick: function (self, record, item, index, e, eOpts) {
                    T7LLastRec = record;
                },
                selectionchange: function (model, records) {
                    if (records.length != 0) {
                        T7LLastRec = records[0];
                    }
                    if (T7LLastRec == null) {
                        T7RGrid.down('#JoinCol').setDisabled(true);
                    }
                    else {
                        T7RGrid.down('#JoinCol').setDisabled(false);
                    }
                }
            }
        });

        //右視窗Store (已選欄位選單)
        var T7RStore = viewModel_VM.getStore('AB0038VM_R');

        //右視窗Tool(已選欄位選單)
        var T7RTool = Ext.create('Ext.PagingToolbar', {
            store: T7RStore,
            displayInfo: true,
            border: false,
            plain: true
        });

        //右視窗Query(已選欄位選單)
        var T7RQuery = Ext.widget({
            xtype: 'form',
            layout: 'form',
            border: false,
            autoScroll: true, // 若為true,容器內容超出容器大小時出現捲動條;若為false超出容器的部分會被裁切掉
            fieldDefaults: {
                xtype: 'textfield',
                labelAlign: 'right'
            },
            items: [{
                xtype: 'panel',
                id: 'PanelP7R',
                border: false,
                layout: 'hbox',
                items: [{
                    xtype: 'button',
                    itemId: 'JoinCol',
                    text: '加入',
                    handler: function () {
                        msglabel("");
                        ColSubmitUrl = '/api/AB0038/JoinCol';
                        ActionFlag = 'J';
                        ColSubmit(T7LLastRec.data.SEQ, T7LLastRec.data.ENAME, T7LLastRec.data.CNAME);
                    }
                }, {
                    xtype: 'button',
                    text: '刪除',
                    itemId: 'DelCol',
                    handler: function () {
                        msglabel("");
                        ColSubmitUrl = '/api/AB0038/DelCol';
                        ActionFlag = 'D';
                        ColSubmit(T7RLastRec.data.SEQ, T7RLastRec.data.ENAME, T7RLastRec.data.CNAME);
                    }
                }, {
                    xtype: 'button',
                    text: '上移',
                    itemId: 'UpCol',
                    handler: function () {
                        msglabel("");
                        ColSubmitUrl = '/api/AB0038/UpCol';
                        ActionFlag = 'U';
                        ColSubmit(T7RLastRec.data.SEQ, T7RLastRec.data.ENAME, T7RLastRec.data.CNAME);
                    }
                }, {
                    xtype: 'button',
                    text: '下移',
                    itemId: 'DownCol',
                    handler: function () {
                        msglabel("");
                        ColSubmitUrl = '/api/AB0038/DownCol';
                        ActionFlag = 'W';
                        ColSubmit(T7RLastRec.data.SEQ, T7RLastRec.data.ENAME, T7RLastRec.data.CNAME);
                    }
                }
                ]
            }]
        });

        //右視窗Grid (已選欄位選單)
        var T7RGrid = Ext.create('Ext.grid.Panel', {
            //title: T1Name,
            store: T7RStore,
            plain: true,
            loadingText: '處理中...',
            loadMask: true,
            cls: 'T1',
            dockedItems: [{
                dock: 'top',
                xtype: 'toolbar',
                layout: 'fit',
                items: [T7RQuery]
            }],
            columns: [{
                text: "順序",
                dataIndex: 'SEQ',
                width: 40
            }, {
                text: "中文名稱",
                dataIndex: 'CNAME',
                width: 200
            }, {
                text: "英文名稱",
                dataIndex: 'ENAME',
                width: 230
            },
            {
                header: "",
                flex: 1
            }],
            viewConfig: {
                listeners: {
                    refresh: function (view) {
                        if ((T7RLastRec == null) || (T7RLastRec.data.SEQ == '0')) {
                            T7RGrid.down('#UpCol').setDisabled(true);
                        }
                        else {
                            T7RGrid.down('#UpCol').setDisabled(false);
                        }
                        if ((T7RLastRec == null) || (T7RLastRec.data.SEQ == T7RGrid.getStore().totalCount - 1)) {
                            T7RGrid.down('#DownCol').setDisabled(true);
                        }
                        else {
                            T7RGrid.down('#DownCol').setDisabled(false);
                        }
                        T7RGrid.down('#DelCol').setDisabled(true);
                        
                        if (ActionFlag == 'D') {
                            if (T7RStore.totalCount == parseInt(T7RLastRec.data.SEQ)) {
                                T7RGrid.getSelectionModel().select((T7RLastRec.data.SEQ) - 1);
                            }
                            else {
                                T7RGrid.getSelectionModel().select(parseInt(T7RLastRec.data.SEQ));
                            }
                        }
                        else if (ActionFlag == 'J') {
                            if (T7LStore.totalCount == T7LLastRec.removedFrom) {
                                T7LGrid.getSelectionModel().select((T7LLastRec.removedFrom) - 1);
                            }
                            else {
                                T7LGrid.getSelectionModel().select(parseInt(T7LLastRec.removedFrom));
                            }
                        }
                    }
                }
            },
            listeners: {
                itemclick: function (self, record, item, index, e, eOpts) {
                    T7RLastRec = record;
                },
                selectionchange: function (model, records) {
                    if (records.length != 0) {
                        T7RLastRec = records[0];
                        if ((T7RLastRec == null) || (T7RLastRec.data.SEQ == '0')) {
                            T7RGrid.down('#UpCol').setDisabled(true);
                        }
                        else {
                            T7RGrid.down('#UpCol').setDisabled(false);
                        }

                        if ((T7RLastRec == null) || (T7RLastRec.data.SEQ == T7RGrid.getStore().totalCount - 1)) {
                            T7RGrid.down('#DownCol').setDisabled(true);
                        }
                        else {
                            T7RGrid.down('#DownCol').setDisabled(false);
                        }
                    }
                    if (T7RLastRec == null) {
                        T7RGrid.down('#DelCol').setDisabled(true);
                    }
                    else {
                        T7RGrid.down('#DelCol').setDisabled(false);
                    }
                }
            }
        });

        //整體彈出視窗
        var newApplyWindow = Ext.create('Ext.window.Window', {
            renderTo: Ext.getBody(),
            modal: true,
            items: [
                {
                    xtype: 'container',
                    layout: 'hbox',
                    items: [
                        {
                            xtype: 'panel',
                            itemId: 't7LGrid',
                            region: 'center',
                            layout: 'fit',
                            collapsible: false,
                            title: '',
                            border: false,
                            width: '50%',
                            height: windowHeight - 64,
                            items: [T7LGrid]
                        }, {
                            xtype: 'panel',
                            itemId: 't7RGrid',
                            region: 'east',
                            layout: 'fit',
                            collapsible: false,
                            title: '',
                            border: false,
                            width: '50%',
                            height: windowHeight - 64,
                            items: [T7RGrid]
                        }
                    ],
                }
            ],
            width: "1000px",
            height: windowHeight,
            resizable: false,
            closable: false,
            y: 0,
            title: "基本檔上傳欄位設定",
            buttonAlign: 'center',
            buttons: [{
                text: '確定',
                handler: function () {
                    this.up('window').hide();
                    //myMask.hide();

                    var T1Column = []; //T1、T3、T4、T6
                    T1Column.push({
                        xtype: 'rownumberer',
                        width: 30,
                        align: 'Center',
                        labelAlign: 'Center'
                    });
                    T7RStore.each(function (rec) {
                        T1Column.push({
                            xtype: 'gridcolumn',
                            dataIndex: rec.get('ENAME'),
                            text: rec.get('CNAME')
                        });
                    });
                    T1Grid.reconfigure(undefined, T1Column);
                    T3Grid.reconfigure(undefined, T1Column);
                    T4Grid.reconfigure(undefined, T1Column);
                    T6Grid.reconfigure(undefined, T1Column);

                    var T2Column = []; //T2
                    T2Column.push({
                        xtype: 'rownumberer',
                        width: 30,
                        align: 'Center',
                        labelAlign: 'Center'
                    }, {
                            xtype: 'gridcolumn',
                            dataIndex: 'CHECK_MSG',
                            width: 150,
                            text: '檢核訊息'
                        });
                    T7RStore.each(function (rec) {
                        T2Column.push({
                            xtype: 'gridcolumn',
                            dataIndex: rec.get('ENAME'),
                            text: rec.get('CNAME')
                        });
                    });
                    T2Grid.reconfigure(undefined, T2Column);

                    var T5Column = []; //T5
                    T5Column.push({
                        xtype: 'rownumberer',
                        width: 30,
                        align: 'Center',
                        labelAlign: 'Center'
                    }, {
                            xtype: 'gridcolumn',
                            dataIndex: 'LOAD_MSG',
                            width: 500,
                            text: '載入訊息'
                        });
                    T5Grid.reconfigure(undefined, T5Column);
                }
            }],
            listeners: {
                show: function (self, eOpts) {
                    newApplyWindow.setY(0);
                }
            }
        });

        //加入/刪除/上移/下移
        function ColSubmit(SEQ, ENAME, CNAME) {
            Ext.Ajax.request({
                url: ColSubmitUrl,
                method: reqVal_p,
                params: {
                    SEQ: SEQ,
                    ENAME: ENAME
                },
                success: function (response) {
                    if ((ActionFlag == 'J') || (ActionFlag == 'D')) {
                        T7LStore.removeAll();
                        T7LLoad();
                        T7RStore.removeAll();
                        T7RLoad();
                    }
                    else if (ActionFlag == 'U') {
                        T7RGrid.getStore().data.items[SEQ].data.ENAME = T7RGrid.getStore().data.items[SEQ - 1].data.ENAME;
                        T7RGrid.getStore().data.items[SEQ].data.CNAME = T7RGrid.getStore().data.items[SEQ - 1].data.CNAME;
                        T7RGrid.getStore().data.items[SEQ - 1].data.ENAME = ENAME;
                        T7RGrid.getStore().data.items[SEQ - 1].data.CNAME = CNAME;
                        T7RGrid.getView().refresh();
                        T7RGrid.getSelectionModel().select(parseInt(T7RLastRec.data.SEQ) - 1);
                    }
                    else if (ActionFlag == 'W') {
                        T7RGrid.getStore().data.items[SEQ].data.ENAME = T7RGrid.getStore().data.items[parseInt(SEQ) + 1].data.ENAME;
                        T7RGrid.getStore().data.items[SEQ].data.CNAME = T7RGrid.getStore().data.items[parseInt(SEQ) + 1].data.CNAME;
                        T7RGrid.getStore().data.items[parseInt(SEQ) + 1].data.ENAME = ENAME;
                        T7RGrid.getStore().data.items[parseInt(SEQ) + 1].data.CNAME = CNAME;
                        T7RGrid.getView().refresh();
                        T7RGrid.getSelectionModel().select(parseInt(T7RLastRec.data.SEQ) + 1);
                    }
                },
                failure: function (response, options) {
                }
            });
        }
    }
    //＊＊＊＊＊＊＊＊＊＊＊＊＊＊＊＊＊ LOAD ＊＊＊＊＊＊＊＊＊＊＊＊＊＊＊＊＊
    {
        function T1Load() {
            T1Tool.moveFirst();
        }
        function T2Load() {
            T2Tool.moveFirst();
        }
        function T7LLoad() {
            T7LStore.getProxy().setExtraParam("P0", T7LQuery.getForm().findField('P0').getValue());
            T7LTool.moveFirst();
        }
        function T7RLoad() {
            T7RTool.moveFirst();
        }
    }
    //＊＊＊＊＊＊＊＊＊＊＊＊＊＊＊＊ 起始執行 ＊＊＊＊＊＊＊＊＊＊＊＊＊＊＊＊
    T7LQuery.getForm().findField('P0').focus(); //讓游標停在P0這一格
    newApplyWindow.show();
    DeleteTemp();
});
