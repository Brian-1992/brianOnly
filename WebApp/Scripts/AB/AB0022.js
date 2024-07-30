Ext.Loader.setConfig({
    enabled: true,
    paths: {
        'WEBAPP': '/Scripts/app'
    }
});

Ext.require(['WEBAPP.utils.Common']);


Ext.onReady(function () {
    var T1Set = '';
    var T1LastRec, T2LastRec;
    var T1Name = '申請繳回';
    var userId = session["UserId"];
    var userName = session['UserName']
    var userInid = session['Inid'];
    var userInidName = session['InidName']
    var defaultFrwh = null;
    var defaultTowh = null;
    var masterStatus = "";
    var reportUrl = '/Report/A/AB0022.aspx';

    var tempT1Record = null;

    var viewModel = Ext.create('WEBAPP.store.AB.AB0022VM');
    var T1Store = viewModel.getStore('ME_DOCM');
    T1Store.on('load', function (store, options) {
        
        var index = -1;
        if (tempT1Record != null) {
            for (var i = 0; i < store.data.items.length; i++) {
                if (tempT1Record.data.DOCNO == store.data.items[i].get('DOCNO')) {
                    index = i;
                }
            }
        }
        if (index > -1) {
            T1Grid.getSelectionModel().select(index, true);
            if (store.data.items[index].get('LIST_ID') == 'Y') {
                Ext.getCmp('exp').enable();
            } else {
                Ext.getCmp('exp').disable();
            }
            T1LastRec = Object.assign({}, tempT1Record);
            tempT1Record = null;

            Ext.getCmp('btnUpdate').setDisabled(false);
            Ext.getCmp('btnDel').setDisabled(false);

            Ext.getCmp('btnUpdate2').setDisabled(true);
            Ext.getCmp('btnDel2').setDisabled(true);
            Ext.getCmp('btnAdd2').setDisabled(false);
            Ext.getCmp('btnSubmit').setDisabled(false);
            Ext.getCmp('btnCopy').setDisabled(false);
        }
    });
    function T1Load() {
        T1Store.removeAll();

        T1Store.getProxy().setExtraParam("p1", T1Query.getForm().findField('P1').getValue());
        T1Store.getProxy().setExtraParam("p2", T1Query.getForm().findField('P2').getValue());
        //T1Store.getProxy().setExtraParam("p3", session['Inid']);
        //T1Store.getProxy().setExtraParam("p3", T1Query.getForm().findField('P3').getValue());
        T1Store.getProxy().setExtraParam("p4", T1Query.getForm().findField('P4').getValue());
        T1Store.getProxy().setExtraParam("p5", T1Query.getForm().findField('P5').getValue());
        T1Store.getProxy().setExtraParam("d0", T1Query.getForm().findField('D0').getValue());
        T1Store.getProxy().setExtraParam("d1", T1Query.getForm().findField('D1').getValue());
        T1Store.getProxy().setExtraParam("FLOWID", T1Query.getForm().findField('FLOWID').getValue());

        T1Store.getProxy().setExtraParam("APPDEPT", userInid);
        T1Store.getProxy().setExtraParam("DOCTYPE", 'RN');
        //T1Store.getProxy().setExtraParam("FLOWID", '0601');
        T1Tool.moveFirst();
        //T1Store.loadPage(1);
        //T1Store.load({
        //    params: {
        //        start: 0
        //    }
        //});
    }

    var T2Store = viewModel.getStore('ME_DOCD');
    function T2Load() {
        if (T1LastRec != null && T1LastRec.data["DOCNO"] !== '') {
            T2Store.getProxy().setExtraParam("p0", T1LastRec.data["DOCNO"]);
            T2Store.load({
                params: {
                    start: 0
                }
            });
        }
    }

    var FrwhStore = viewModel.getStore('FRWH');
    function FrwhLoad() {
        //TowhStore.getProxy().setExtraParam("INID", session['Inid']);
        FrwhStore.getProxy().setExtraParam("start", 0);
        FrwhStore.load(function (records, operation, success) {
            if (records.length > 0) {
                //
                defaultFrwh = records[0].data;

                Ext.Ajax.request({
                    url: '/api/AB0010/GetGrade',
                    method: reqVal_p,
                    params: {
                        //WH_NO: newValue
                        WH_NO: defaultFrwh.VALUE
                    },
                    success: function (response) {
                        //
                        var data = Ext.decode(response.responseText);
                        TowhLoad(data);
                    },
                    failure: function (response) {
                        Ext.MessageBox.alert('錯誤', '發生例外錯誤');
                    },
                    //jsonData: data
                });
            }
        });
    }

    var TowhStore = viewModel.getStore('TOWH');
    function TowhLoad(wh_grade) {
        TowhStore.getProxy().setExtraParam("INID", session['Inid']);
        TowhStore.getProxy().setExtraParam("WH_GRADE", wh_grade);
        TowhStore.getProxy().setExtraParam("start", 0);
        //
        TowhStore.load(function (records, operation, success) {
            if (records.length > 0) {
                //
                defaultTowh = records[0].data;

                var x = T1Form.getForm().findField('x').getValue();
                if (x === "I") {
                    T1Form.getForm().findField('TOWH').setValue(defaultTowh.WH_NO);
                }
            }
        });
    }
    // 庫房清單
    var queryFrwhStore = Ext.create('Ext.data.Store', {
        fields: ['VALUE', 'COMBITEM']
    });
    function QueryFrwhLoad(wh_grade) {
        TowhStore.getProxy().setExtraParam("INID", session['Inid']);
        TowhStore.getProxy().setExtraParam("WH_GRADE", wh_grade);

        Ext.Ajax.request({
            url: '/api/AB0022/GetFrwhCombo',
            //params: {
            //    INID: session['Inid'],
            //    WH_GRADE: wh_grade
            //},
            method: reqVal_p,
            success: function (response) {
                var data = Ext.decode(response.responseText);
                if (data.success) {
                    var wh_nos = data.etts;
                    queryFrwhStore.removeAll();
                    if (wh_nos.length > 0) {
                        queryFrwhStore.add({ VALUE: '', COMBITEM: '' });
                        for (var i = 0; i < wh_nos.length; i++) {
                            queryFrwhStore.add({ VALUE: wh_nos[i].VALUE, COMBITEM: wh_nos[i].COMBITEM });
                        }
                    }
                }
            },
            failure: function (response, options) {

            }
        });
    }
    var queryTowhStore = Ext.create('Ext.data.Store', {
        fields: ['WH_NO', 'WH_NAME']
    });
    function QueryTowhLoad(wh_grade) {
        TowhStore.getProxy().setExtraParam("INID", session['Inid']);
        TowhStore.getProxy().setExtraParam("WH_GRADE", wh_grade);

        Ext.Ajax.request({
            url: '/api/AB0022/GetTowh',
            params: {
                INID: session['Inid'],
                WH_GRADE: wh_grade
            },
            method: reqVal_p,
            success: function (response) {
                var data = Ext.decode(response.responseText);
                if (data.success) {
                    var wh_nos = data.etts;
                    queryTowhStore.removeAll();
                    if (wh_nos.length > 0) {
                        queryTowhStore.add({ WH_NO: '', WH_NAME: '' });
                        for (var i = 0; i < wh_nos.length; i++) {
                            queryTowhStore.add({ WH_NO: wh_nos[i].WH_NO, WH_NAME: wh_nos[i].WH_NAME });
                        }
                    }
                }
            },
            failure: function (response, options) {

            }
        });
    }
    // 申請人清單
    var queryAppidStore = Ext.create('Ext.data.Store', {
        fields: ['VALUE', 'COMBITEM']
    });
    function QueryAppidLoad() {
        Ext.Ajax.request({
            url: '/api/AB0022/GetQueryAppidCombo',
            method: reqVal_g,
            success: function (response) {
                var data = Ext.decode(response.responseText);
                if (data.success) {
                    var users = data.etts;
                    queryAppidStore.removeAll();
                    if (users.length > 0) {
                        queryAppidStore.add({ VALUE: '', COMBITEM: '' });
                        for (var i = 0; i < users.length; i++) {
                            queryAppidStore.add({ VALUE: users[i].VALUE, COMBITEM: users[i].COMBITEM });
                        }
                    }
                }
            },
            failure: function (response, options) {

            }
        });
    }

    var MatClassStore = viewModel.getStore('MAT_CLASS');
    function MatClassLoad() {
        MatClassStore.load({
            params: {
                start: 0
            }
        });
    }

    function deleteEmptyMaster() {
        Ext.Ajax.request({
            url: '/api/AB0022/DeleteEmptyMaster',
            method: reqVal_p,
            success: function (response) {
                var data = Ext.decode(response.responseText);
                if (data.success) {
                    console.log("delete rows:" + data.afrs);
                }

            },
            failure: function (response, options) {

            }
        });
    }
    deleteEmptyMaster();


    // 查詢欄位
    var mLabelWidth = 90;
    var mWidth = 230;
    var T1Query = Ext.widget({
        xtype: 'form',
        layout: 'form',
        border: false,
        autoScroll: true, // 若為true,容器內容超出容器大小時出現捲動條;若為false超出容器的部分會被裁切掉
        fieldDefaults: {
            xtype: 'textfield',
            labelAlign: 'right',
            labelWidth: mLabelWidth,
            width: mWidth
        },
        items: [
            {
                xtype: 'textfield',
                fieldLabel: '申請單號',
                name: 'P1',
                id: 'P1',
                enforceMaxLength: true,
                maxLength: 21,
                width: 300,
                labelWidth: 90,
                padding: '0 4 0 4'
            },
            {
                //xtype: 'textfield',
                xtype: 'datefield',
                fieldLabel: '申請日期',
                name: 'D0',
                id: 'D0',
                regex: /\d{7,7}/,
                labelWidth: 90,
                enforceMaxLength: true, // 限制可輸入最大長度
                maxLength: 7, // 可輸入最大長度為7
                padding: '0 4 0 4'
            },
            {
                //xtype: 'textfield',
                xtype: 'datefield',
                fieldLabel: '至',
                name: 'D1',
                id: 'D1',
                labelSeparator: '',
                regex: /\d{7,7}/,
                labelWidth: 90,
                enforceMaxLength: true, // 限制可輸入最大長度
                maxLength: 7, // 可輸入最大長度為7
                padding: '0 4 0 4'
            },
            {
                xtype: 'textfield',
                fieldLabel: '申請人',
                name: 'P2',
                id: 'P2',
                enforceMaxLength: true,
                maxLength: 21,
                width: 300,
                labelWidth: 90,
                padding: '0 4 0 4'
                //xtype: 'combo',
                //store: queryAppidStore,
                //name: 'P2',
                //id: 'P2',
                //fieldLabel: '申請人',
                //displayField: 'COMBITEM',
                //valueField: 'VALUE',
                //anyMatch: true,
                //typeAhead: true,
                //queryMode: 'local',
                //triggerAction: 'all',
                //multiSelect: false,
                //tpl: '<tpl for="."><div class="x-boundlist-item" style="height:auto;">{COMBITEM}&nbsp;</div></tpl>',
            },
            {
                //xtype: 'textfield',
                //fieldLabel: '申請庫房',
                //name: 'P4',
                //id: 'P4',
                //enforceMaxLength: true,
                //maxLength: 21,
                //width: 300,
                //labelWidth: 90,
                //padding: '0 4 0 4'
                xtype: 'combo',
                store: queryFrwhStore,
                name: 'P4',
                id: 'P4',
                fieldLabel: '申請庫房',
                displayField: 'COMBITEM',
                valueField: 'VALUE',
                anyMatch: true,
                typeAhead: true,
                queryMode: 'local',
                triggerAction: 'all',
                multiSelect: false,
                tpl: '<tpl for="."><div class="x-boundlist-item" style="height:auto;">{COMBITEM}&nbsp;</div></tpl>',
                listeners: {
                    select: function (combo, record, eOpts) {
                        

                        if (record.data.VALUE == '') {
                            Ext.getCmp('P5').disable();
                            Ext.getCmp('P5').setValue('');
                            return;
                        }

                        Ext.getCmp('P5').setValue('');
                        Ext.getCmp('P5').enable();
                        Ext.Ajax.request({
                            url: '/api/AB0010/GetGrade',
                            method: reqVal_p,
                            params: {
                                //WH_NO: newValue
                                WH_NO: record.get("VALUE")
                            },
                            success: function (response) {
                                
                                var data = Ext.decode(response.responseText);
                                QueryTowhLoad(data);
                            },
                            failure: function (response) {
                                Ext.MessageBox.alert('錯誤', '發生例外錯誤');
                            },
                            //jsonData: data
                        });
                    }
                }
            },
            {
                //xtype: 'textfield',
                //fieldLabel: '繳回庫房',
                //name: 'P5',
                //id: 'P5',
                //enforceMaxLength: true,
                //maxLength: 21,
                //width: 300,
                //labelWidth: 90,
                //padding: '0 4 0 4'
                xtype: 'combo',
                store: queryTowhStore,
                name: 'P5',
                id: 'P5',
                fieldLabel: '繳回庫房',
                displayField: 'WH_NAME',
                valueField: 'WH_NO',
                anyMatch: true,
                typeAhead: true,
                queryMode: 'local',
                triggerAction: 'all',
                multiSelect: false,
                tpl: '<tpl for="."><div class="x-boundlist-item" style="height:auto;">{WH_NAME}&nbsp;</div></tpl>',
            },
            {
                xtype: 'checkboxgroup',
                fieldLabel: '狀態',
                name: 'FLOWID',
                columns: 1,
                vertical: true,
                items: [
                    { boxLabel: '申請中', name: 'rb', inputValue: '0401', checked: true },
                    { boxLabel: '點收中', name: 'rb', inputValue: '0402', checked: true },
                    { boxLabel: '已結案', name: 'rb', inputValue: '0499' }
                ]
            }
        ],
        buttons: [
            {
                itemId: 'query',
                text: '查詢',
                handler: function () {
                    T2Store.removeAll();
                    T1Grid.getSelectionModel().deselectAll();
                    T1Load();
                    Ext.getCmp('eastform').collapse();
                    msglabel('訊息區:');

                    Ext.getCmp('btnUpdate').setDisabled(true);
                    Ext.getCmp('btnUpdate2').setDisabled(true);
                    Ext.getCmp('btnDel').setDisabled(true);
                    Ext.getCmp('btnDel2').setDisabled(true);
                    Ext.getCmp('btnAdd2').setDisabled(true);
                    Ext.getCmp('btnSubmit').setDisabled(true);
                    Ext.getCmp('btnCopy').setDisabled(true);
                }
            },
            {
                itemId: 'clean',
                text: '清除',
                handler: function () {
                    var f = this.up('form').getForm();
                    f.reset();
                    f.findField('P1').focus(); // 進入畫面時輸入游標預設在P0
                    msglabel('訊息區:');
                }
            }
        ]
    });

    var T1Tool = Ext.create('Ext.PagingToolbar', {
        store: T1Store,
        displayInfo: true,
        border: false,
        plain: true,
        buttons: [
            {
                text: '新增',
                id: 'btnAdd',
                name: 'btnAdd',
                handler: function () {
                    if ((defaultFrwh == null) || (defaultFrwh == '')) {
                        Ext.Msg.alert('提醒', '此帳號無對應的庫房資料，無法進行新增');
                        msglabel('此帳號無對應的庫房資料，無法進行新增');
                    }
                    else {
                        setFormVisible(true, false);
                        T1Set = '/api/AB0022/MasterCreate';
                        setFormT1('I', '新增');
                    }
                }
            },
            {
                text: '修改',
                id: 'btnUpdate',
                name: 'btnUpdate',
                handler: function () {

                    setFormVisible(true, false);

                    T1Set = '/api/AB0022/MasterUpdate';
                    setFormT1('U', '修改');
                }
            },
            {
                text: '刪除',
                id: 'btnDel',
                name: 'btnDel',
                handler: function () {
                    var selection = T1Grid.getSelection();
                    if (selection.length) {
                        let name = '';
                        let docno = '';
                        //selection.map(item => {
                        //    name += '「' + item.get('DOCNO') + '」<br>';
                        //    docno += item.get('DOCNO') + ',';
                        //});
                        $.map(selection, function (item, key) {
                            name += '「' + item.get('DOCNO') + '」<br>';
                            docno += item.get('DOCNO') + ',';
                        })

                        Ext.MessageBox.confirm('刪除', '是否確定刪除申請單號?<br>' + name, function (btn, text) {
                            if (btn === 'yes') {
                                Ext.Ajax.request({
                                    url: '/api/AB0022/MasterDelete',
                                    method: reqVal_p,
                                    params: {
                                        DOCNO: docno
                                    },
                                    success: function (response) {
                                        var data = Ext.decode(response.responseText);
                                        if (data.success) {
                                            msglabel('訊息區:刪除成功');
                                            T2Store.removeAll();
                                            T1Grid.getSelectionModel().deselectAll();
                                            T1Load();
                                            
                                            setFormVisible(true, false);
                                            Tabs.setActiveTab('Query');
                                            //Ext.getCmp('btnSubmit').setDisabled(true);
                                        }
                                        else {
                                            Ext.MessageBox.alert('錯誤', data.msg);
                                            msglabel('訊息區:' + data.msg);
                                        }
                                    },
                                    failure: function (response) {
                                        Ext.MessageBox.alert('錯誤', '發生例外錯誤');
                                    }
                                });
                            }
                        });
                    }
                }
            },
            {
                text: '複製',
                id: 'btnCopy',
                name: 'btnCopy',
                handler: function () {
                    var selection = T1Grid.getSelection();
                    if (selection.length) {
                        let name = '';
                        let docno = '';
                        //selection.map(item => {
                        //    name += '「' + item.get('DOCNO') + '」<br>';
                        //    docno += item.get('DOCNO') + ',';
                        //});
                        $.map(selection, function (item, key) {
                            name += '「' + item.get('DOCNO') + '」<br>';
                            docno += item.get('DOCNO') + ',';
                        })

                        Ext.MessageBox.confirm('訊息', '確認是否複製?<br>單號如下<br>' + name, function (btn, text) {
                            if (btn === 'yes') {
                                Ext.Ajax.request({
                                    url: '/api/AB0022/Copy',
                                    method: reqVal_p,
                                    params: {
                                        DOCNO: docno
                                    },
                                    //async: true,
                                    success: function (response) {
                                        
                                        var data = Ext.decode(response.responseText);
                                        if (data.success) {
                                            msglabel('訊息區:複製成功');
                                            T2Store.removeAll();
                                            T1Grid.getSelectionModel().deselectAll();
                                            //T1Query.reset();
                                            T1Load();
                                            Ext.getCmp('btnUpdate').setDisabled(true);
                                            Ext.getCmp('btnDel').setDisabled(true);
                                            Ext.getCmp('btnSubmit').setDisabled(true);
                                            Ext.getCmp('btnCopy').setDisabled(true);
                                        }
                                        else
                                            Ext.MessageBox.alert('錯誤', data.msg);
                                    },
                                    failure: function (response) {
                                        Ext.MessageBox.alert('錯誤', '發生例外錯誤');
                                    }
                                });
                            }
                        });
                    }
                    else {
                        Ext.MessageBox.alert('錯誤', '尚未選取單據號碼');
                        return;
                    }
                }
            },
            {
                text: '提出申請',
                id: 'btnSubmit',
                name: 'btnSubmit',
                handler: function () {
                    //if (T1LastRec == null || T1LastRec.data["DOCNO"] == '') {
                    //    Ext.MessageBox.alert('錯誤', '尚未選取單據號碼');
                    //    return;
                    //}
                    var selection = T1Grid.getSelection();
                    if (selection.length) {
                        let name = '';
                        let docno = '';
                        //selection.map(item => {
                        //    name += '「' + item.get('DOCNO') + '」<br>';
                        //    docno += item.get('DOCNO') + ',';
                        //});
                        $.map(selection, function (item, key) {
                            name += '「' + item.get('DOCNO') + '」<br>';
                            docno += item.get('DOCNO') + ',';
                        })

                        Ext.MessageBox.confirm('訊息', '確認是否申請?<br>單號如下<br>' + name, function (btn, text) {
                            if (btn === 'yes') {
                                var myMask = new Ext.LoadMask(viewport, { msg: '處理中...' });
                                myMask.show();

                                Ext.Ajax.request({
                                    url: '/api/AB0022/UpdateStatusBySP',
                                    method: reqVal_p,
                                    params: {
                                        DOCNO: docno,
                                        FLOWID: '0402'
                                    },
                                    //async: true,
                                    success: function (response) {
                                        
                                        var data = Ext.decode(response.responseText);
                                        if (data.success) {
                                            msglabel('訊息區:送出成功');
                                            T2Store.removeAll();
                                            T1Grid.getSelectionModel().deselectAll();
                                            T1Load();
                                            Ext.getCmp('btnUpdate').setDisabled(true);
                                            Ext.getCmp('btnDel').setDisabled(true);
                                            Ext.getCmp('btnSubmit').setDisabled(true);
                                            Ext.getCmp('btnCopy').setDisabled(true);
                                        }
                                        else {
                                            Ext.MessageBox.alert('錯誤', data.msg);
                                        }
                                        myMask.hide();

                                    },
                                    failure: function (response) {
                                        
                                        Ext.MessageBox.alert('錯誤', '發生例外錯誤');
                                        myMask.hide();
                                    }
                                });
                            }
                        });
                    }
                    else {
                        Ext.MessageBox.alert('錯誤', '尚未選取單據號碼');
                        return;
                    }
                }
            },
            {
                text: '效期',
                id: 'exp',
                disabled: true,
                //hidden: true,
                handler: function () {
                    showExpWindow(T1LastRec.data.DOCNO, 'O', viewport);
                }
            },
            {
                text: '列印',
                id: 'btnPrint',
                disabled: true,
                handler: function () {
                    var selection = T1Grid.getSelection();
                    if (selection.length == 0) {
                        Ext.Msg.alert('提醒', '請選擇要列印的申請單');
                        return;
                    }
                    var docnos = '';
                    for (var i = 0; i < selection.length; i++) {
                        if (docnos != '') {
                            docnos += '|';
                        }
                        docnos += selection[i].data.DOCNO
                    }

                    showReport(docnos);
                }
            },
        ]
    });

    function showReport(docnos) {

        if (!win) {
            var winform = Ext.create('Ext.form.Panel', {
                id: 'iframeReport',
                //height: '100%',
                //width: '100%',
                layout: 'fit',
                closable: false,
                html: '<iframe src="' + reportUrl + '?DOCNOS=' + docnos
                + '" id="mainContent" name="mainContent" width="100%" height="100%" frameborder="0" style="background-color:#FFFFFF"></iframe>',
                buttons: [{
                    text: '關閉',
                    handler: function () {
                        this.up('window').destroy();
                    }
                }]
            });
            var win = GetPopWin(viewport, winform, '', viewport.width - 300, viewport.height - 20);
        }
        win.show();
    }

    // 按'新增'或'修改'時的動作
    function setFormT1(x, t) {
        viewport.down('#t1Grid').mask();
        viewport.down('#t2Grid').mask();
        viewport.down('#form').setTitle(t);
        viewport.down('#form').expand();
        Tabs.setActiveTab('Form');
        var currentTab = Tabs.getActiveTab();
        currentTab.setTitle('填寫');

        var f = T1Form.getForm();
        if (x === "I") {

            isNew = true;
            var r = Ext.create('WEBAPP.model.ME_DOCM'); // /Scripts/app/model/MiUnitexch.js
            T1Form.loadRecord(r); // 建立空白model,在新增時載入T1Form以清空欄位內容
            u = f.findField("FRWH");
            u.setReadOnly(false);
            u.clearInvalid();

            f.findField('FRWH').setReadOnly(false);
            f.findField('FRWH').setValue(defaultFrwh.VALUE);
            f.findField('TOWH').setReadOnly(false);
            //f.findField('TOWH').setValue(defaultTowh.WH_NO);
            
            f.findField("DOCNO").setValue('系統自編');
            f.findField("APPID").setValue(userName);
            f.findField("INID_NAME").setValue(userInid + ' ' + userInidName);
            f.findField("APPTIME").setValue(getChtToday());
            f.findField('FRWH').clearInvalid();
            f.findField('TOWH').clearInvalid();

            Ext.Ajax.request({
                url: '/api/AB0010/GetGrade',
                method: reqVal_p,
                params: {
                    WH_NO: f.findField('FRWH').getValue()
                },
                success: function (response) {
                    var data = Ext.decode(response.responseText);
                    //
                    TowhLoad(data);

                    //tmpArray = T1LastRec.data["TOWH_NAME"].split(" ");

                    //// 切換T1Grid時, 如果值一樣,還做setValue()的話,欄位會被清空,所以值不同再做setValue()
                    //if (f.findField('TOWH').getValue() != tmpArray[0])
                    //    f.findField('TOWH').setValue(tmpArray[0]);
                },
                failure: function (response) {
                    Ext.MessageBox.alert('錯誤', '發生例外錯誤');
                },
            });



        }
        else {
            f.findField("DOCNO").setValue(T1LastRec.data["DOCNO"]);
            f.findField("APPID").setValue(T1LastRec.data["APP_NAME"]);
            f.findField("INID_NAME").setValue(T1LastRec.data["APPDEPT_NAME"]);
            f.findField("APPTIME").setValue(T1LastRec.data["APPTIME"]);

            u = f.findField('FRWH');
            u.setReadOnly(false);
            f.findField('TOWH').setReadOnly(false);

            var tmpArray = T1LastRec.data["FRWH_NAME"].split(" ");
            f.findField("FRWH").setValue(tmpArray[0]);

            // 因為領用部門和核撥庫房有連動, 所以都要重新取得
            //Ext.Ajax.request({
            //    url: '/api/AB0010/GetGrade',
            //    method: reqVal_p,
            //    params: {
            //        WH_NO: tmpArray[0]
            //    },
            //    success: function (response) {
            //        var data = Ext.decode(response.responseText);
            //        FrwhLoad(data);

            //        tmpArray = T1LastRec.data["FRWH_NAME"].split(" ");
            //        f.findField('FRWH').setValue(tmpArray[0]);
            //    },
            //    failure: function (response) {
            //        Ext.MessageBox.alert('錯誤', '發生例外錯誤');
            //    },
            //});

        }
        f.findField('x').setValue(x);
        //f.findField('MAT_CLASS').setReadOnly(false);
        T1Form.down('#cancel').setVisible(true);
        T1Form.down('#submit').setVisible(true);
        u.focus();
    }

    // 顯示明細/新增/修改輸入欄
    var T1Form = Ext.widget({
        xtype: 'form',
        layout: 'form',
        frame: false,
        cls: 'T1b',
        title: '',
        autoScroll: true,
        bodyPadding: '5 5 0',
        fieldDefaults: {
            labelAlign: 'right',
            msgTarget: 'side',
            labelWidth: 90
        },
        defaultType: 'textfield',
        items: [
            {
                fieldLabel: 'Update',
                name: 'x',
                xtype: 'hidden'
            },
            {
                xtype: 'displayfield',
                fieldLabel: '申請單號',
                name: 'DOCNO',
                enforceMaxLength: true,
                maxLength: 100,
                readOnly: true,
                submitValue: true
            },
            {
                xtype: 'displayfield',
                fieldLabel: '申請日期',
                name: 'APPTIME',
                enforceMaxLength: true,
                maxLength: 100,
                readOnly: true,
                submitValue: true
            },
            {
                xtype: 'displayfield',
                fieldLabel: '申請人員',
                name: 'APPID',
                enforceMaxLength: true,
                maxLength: 100,
                readOnly: true,
                submitValue: true
            },
            {
                xtype: 'displayfield',
                fieldLabel: '申請單位',
                name: 'INID_NAME',
                enforceMaxLength: true,
                maxLength: 100,
                readOnly: true,
                submitValue: true
            },
            {
                xtype: 'combo',
                fieldLabel: '申請庫房',
                name: 'FRWH',
                id: 'FRWH',
                store: FrwhStore,
                displayField: 'COMBITEM',
                valueField: 'VALUE',
                readOnly: true,
                editable: false,
                allowBlank: false, // 欄位為必填
                fieldCls: 'required',
                listeners: {
                    //change: function (e, newValue, oldValue, eOpts) {
                    select: function (combo, record, eOpts) {
                        Ext.getCmp('TOWH').setValue('');
                        Ext.Ajax.request({
                            url: '/api/AB0010/GetGrade',
                            method: reqVal_p,
                            params: {
                                //WH_NO: newValue
                                WH_NO: record.get("VALUE")
                            },
                            success: function (response) {
                                
                                var data = Ext.decode(response.responseText);
                                TowhLoad(data);
                            },
                            failure: function (response) {
                                Ext.MessageBox.alert('錯誤', '發生例外錯誤');
                            },
                            //jsonData: data
                        });
                    }
                }
            },
            {
                xtype: 'combo',
                fieldLabel: '繳回庫房',
                name: 'TOWH',   //TOWH
                id: 'TOWH',
                store: TowhStore,
                displayField: 'WH_NAME',
                valueField: 'WH_NO',
                readOnly: true,
                editable: false,
                allowBlank: false, // 欄位為必填
                //anyMatch: true,
                //typeAhead: true,
                //forceSelection: true,
                //queryMode: 'local',
                //triggerAction: 'all',
                fieldCls: 'required'
            }
        ],
        buttons: [
            {
                itemId: 'submit', text: '儲存', hidden: true,
                handler: function () {
                    if (this.up('form').getForm().isValid()) { // 檢查T1Form填寫資料是否符合規則(必填欄位都有填、輸入內容有符合正規表示式等)
                        //if (this.up('form').getForm().findField('AGEN_NAMEC').getValue() == ''
                        //    && this.up('form').getForm().findField('AGEN_NAMEE').getValue() == '')
                        //    Ext.Msg.alert('提醒', '廠商中文名稱或廠商英文名稱至少需輸入一種');
                        //else {
                        //    var confirmSubmit = viewport.down('#form').title.substring(0, 2);
                        //    Ext.MessageBox.confirm(confirmSubmit, '是否確定' + confirmSubmit + '?', function (btn, text) {
                        //        if (btn === 'yes') {
                        //            T1Submit();
                        //        }
                        //    });
                        //}

                        var confirmSubmit = viewport.down('#form').title.substring(0, 2);
                        Ext.MessageBox.confirm(confirmSubmit, '是否確定' + confirmSubmit + '?', function (btn, text) {
                            if (btn === 'yes') {
                                
                                T1Submit();
                                T1Set = '';
                            }
                        });
                    }
                    else
                        Ext.Msg.alert('提醒', '輸入資料格式有誤');
                }
            },
            {
                itemId: 'cancel', text: '取消', hidden: true, handler: T1Cleanup
            }
        ]
    });

    function T1Submit() {
        var f = T1Form.getForm();
        
        if (f.isValid()) {
            var myMask = new Ext.LoadMask(viewport, { msg: '處理中...' });
            myMask.show();
            f.submit({
                url: T1Set,
                success: function (form, action) {
                    myMask.hide();
                    var f2 = T1Form.getForm();
                    var r = f2.getRecord();
                    switch (f2.findField("x").getValue()) {
                        case "I":
                            T1Query.getForm().reset();
                            var v = action.result.etts[0];
                            T2Store.removeAll();
                            T1Query.getForm().findField('P1').setValue(v.DOCNO);
                            T1Load();
                            msglabel('訊息區:資料新增成功');
                            break;
                        case "U":
                            T1Load();
                            msglabel('訊息區:資料更新成功');
                            break;
                        case "R":
                            T1Load();
                            msglabel('訊息區:資料退回成功');
                            break;
                    }

                    T1Cleanup();
                    Tabs.setActiveTab('Query');
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
                            Ext.Msg.alert('失敗', action.result.msg);
                            break;
                    }
                }
            });
        }
    }

    var T1Grid = Ext.create('Ext.grid.Panel', {
        //title: T1Name,
        store: T1Store,
        plain: true,
        loadingText: '處理中...',
        loadMask: true,
        cls: 'T1',
        //plugins: [T1RowEditing],
        dockedItems: [
            {
                dock: 'top',
                xtype: 'toolbar',
                items: [T1Query]
            },
            {
                dock: 'top',
                xtype: 'toolbar',
                items: [T1Tool]
            }
        ],
        selModel: {
            checkOnly: false,
            injectCheckbox: 'first',
            mode: 'MULTI'
        },
        selType: 'checkboxmodel',
        columns: [
            {
                xtype: 'rownumberer'
            },
            {
                text: "申請單號",
                dataIndex: 'DOCNO',
                width: 100
            },
            {
                text: "狀態",
                dataIndex: 'FLOWID',
                width: 120
            },
            {
                text: "申請人員",
                dataIndex: 'APPID',
                width: 150
            },
            {
                xtype: 'hidden',
                text: "申請人員",
                dataIndex: 'APP_NAME',
                width: 150
            },
            {
                text: "申請部門",
                dataIndex: 'APPDEPT_NAME',
                width: 200
            },
            {
                text: "申請日期",
                dataIndex: 'APPTIME',
                width: 120
            },
            {
                text: "申請庫房",
                dataIndex: 'FRWH_NAME',
                width: 120
            },
            {
                text: "繳回庫房",
                dataIndex: 'TOWH_NAME',
                width: 120
            },
            {
                header: "",
                flex: 1
            }
        ],
        listeners: {
            //click: {
            //    element: 'el',
            //    fn: function () {
            //        if (T1Form.hidden === true) {
            //            T1Form.setVisible(true);
            //            T2Form.setVisible(false);
            //        }

            //        // grid中所有click都會觸發, 所以要判斷真的有選取到一筆record才能執行
            //        if (T1LastRec != null) {

            //            Ext.getCmp('btnUpdate').setDisabled(false);
            //            Ext.getCmp('btnDel').setDisabled(false);

            //            Ext.getCmp('btnUpdate2').setDisabled(true);
            //            Ext.getCmp('btnDel2').setDisabled(true);
            //            Ext.getCmp('btnAdd2').setDisabled(false);
            //            Ext.getCmp('btnSubmit').setDisabled(false);
            //            Ext.getCmp('btnCopy').setDisabled(false);



            //            if (T1Set === '')
            //                setFormT1a();

            //            T2Load();
            //        }

            //    }
            //},
            selectionchange: function (model, records) {
                T1Rec = records.length;

                Ext.getCmp('btnPrint').disable();
                if (records.length == 0) {
                    Ext.getCmp('exp').setDisabled(true);
                    Ext.getCmp('btnUpdate').setDisabled(true);
                    Ext.getCmp('btnDel').setDisabled(true);

                    Ext.getCmp('btnUpdate2').setDisabled(true);
                    Ext.getCmp('btnDel2').setDisabled(true);
                    Ext.getCmp('btnAdd2').setDisabled(true);
                    Ext.getCmp('btnSubmit').setDisabled(true);
                    Ext.getCmp('btnCopy').setDisabled(true);
                }
                if (records.length > 0) {
                    Ext.getCmp('btnPrint').enable();
                    Ext.getCmp('btnCopy').setDisabled(false);
                } 
                if (records.length > 1) {
                    Ext.getCmp('exp').setDisabled(true);
                }
                
                if (T1LastRec == null)
                    Ext.getCmp('btnAdd2').setDisabled(true);
            },
            itemclick: function (self, record, item, index, e, eOpts) {

                if (T1Rec == 0) {
                    T1LastRec = null;
                    return;
                }

                Ext.getCmp('btnUpdate').setDisabled(false);
                Ext.getCmp('btnDel').setDisabled(false);

                Ext.getCmp('btnUpdate2').setDisabled(true);
                Ext.getCmp('btnDel2').setDisabled(true);
                Ext.getCmp('btnAdd2').setDisabled(false);
                Ext.getCmp('btnSubmit').setDisabled(false);
                Ext.getCmp('btnCopy').setDisabled(false);

                Ext.getCmp('eastform').expand();
                msglabel('訊息區:');

                T1LastRec = record;
                tempT1Record = Object.assign({}, record);
                setFormT1a();

                setFormVisible(true, false);

                Ext.getCmp('exp').setDisabled(true);

                if (T1LastRec != null && record.data.LIST_ID == 'Y') {  //if (record.data.LIST_ID == 'Y') {
                    Ext.getCmp('exp').setDisabled(false);
                } else {
                    Ext.getCmp('exp').setDisabled(true);
                }

                Tabs.setActiveTab('Form');

                T2Load();


            }
        }
    });

    // 點選T1Grid一筆資料的動作
    function setFormT1a() {
        if (T1LastRec != null) {
            //viewport.down('#form').setTitle(t + T1Name);
            viewport.down('#form').expand();
            Tabs.setActiveTab('Form');
            var currentTab = Tabs.getActiveTab();
            currentTab.setTitle('瀏覽');
            //
            var f = T1Form.getForm();
            f.findField("DOCNO").setValue(T1LastRec.data["DOCNO"]);
            f.findField("APPID").setValue(T1LastRec.data["APP_NAME"]);
            f.findField("INID_NAME").setValue(T1LastRec.data["APPDEPT_NAME"]);
            var tmpArray = T1LastRec.data["FRWH_NAME"].split(' ');
            f.findField("FRWH").setValue(tmpArray[0]);

            // 因為領用部門和核撥庫房有連動, 所以都要重新取得
            Ext.Ajax.request({
                url: '/api/AB0010/GetGrade',
                method: reqVal_p,
                params: {
                    WH_NO: tmpArray[0]
                },
                success: function (response) {
                    var data = Ext.decode(response.responseText);
                    //
                    TowhLoad(data);

                    tmpArray = T1LastRec.data["TOWH_NAME"].split(" ");

                    // 切換T1Grid時, 如果值一樣,還做setValue()的話,欄位會被清空,所以值不同再做setValue()
                    if (f.findField('TOWH').getValue() != tmpArray[0])
                        f.findField('TOWH').setValue(tmpArray[0]);
                },
                failure: function (response) {
                    Ext.MessageBox.alert('錯誤', '發生例外錯誤');
                },
            });

            f.findField("APPTIME").setValue(T1LastRec.data["APPTIME"]);

            T1Form.down('#cancel').setVisible(false);
            T1Form.down('#submit').setVisible(false);

            tmpArray = T1LastRec.data["APP_NAME"].split(' ');
            tmpArray1 = T1LastRec.data["FLOWID"].split(' ');
            if (session['UserId'] !== tmpArray[0] || tmpArray1[0] !== '0401') {
                Ext.getCmp("btnUpdate").setDisabled(true);
                Ext.getCmp("btnDel").setDisabled(true);
                Ext.getCmp("btnSubmit").setDisabled(true);
                Ext.getCmp("btnAdd2").setDisabled(true);
            }
            else {
                Ext.getCmp("btnUpdate").setDisabled(false);
                Ext.getCmp("btnDel").setDisabled(false);
                Ext.getCmp("btnSubmit").setDisabled(false);
                Ext.getCmp("btnAdd2").setDisabled(false);
            }

        }

    }

    var T2Tool = Ext.create('Ext.PagingToolbar', {
        store: T2Store,
        displayInfo: true,
        border: false,
        plain: true,
        buttons: [
            //{
            //    text: '查詢',
            //    handler: function () {
            //        Ext.getCmp('eastform').expand();
            //        T1Form.setVisible(false);
            //        T1Query.setVisible(true);
            //    }
            //},
            {
                text: '新增',
                id: 'btnAdd2',
                name: 'btnAdd2',
                handler: function () {

                    setFormVisible(false, true);
                    
                    T1Set = '/api/AB0022/DetailCreate';
                    setFormT2('I', '新增');
                }
            },
            {
                text: '修改',
                id: 'btnUpdate2',
                name: 'btnUpdate2',
                handler: function () {

                    setFormVisible(false, true);

                    T1Set = '/api/AB0022/DetailUpdate';
                    setFormT2('U', '修改');

                }
            },
            {
                text: '刪除',
                id: 'btnDel2',
                name: 'btnDel2',
                handler: function () {
                    var selection = T2Grid.getSelection();
                    if (selection.length) {
                        //let name = '';
                        let docno = '';
                        let seq = '';
                        //selection.map(item => {
                        //    //name += '「' + item.get('SEQ') + '」<br>';
                        //    docno += item.get('DOCNO') + ',';
                        //    seq += item.get('SEQ') + ',';
                        //});
                        $.map(selection, function (item, key) {
                            docno += item.get('DOCNO') + ',';
                            seq += item.get('SEQ') + ',';
                        })

                        Ext.MessageBox.confirm('刪除', '是否確定刪除項次?<br>', function (btn, text) {
                            if (btn === 'yes') {
                                Ext.Ajax.request({
                                    url: '/api/AB0022/DetailDelete',
                                    method: reqVal_p,
                                    params: {
                                        DOCNO: docno,
                                        SEQ: seq
                                    },
                                    success: function (response) {
                                        var data = Ext.decode(response.responseText);
                                        if (data.success) {
                                            //Ext.MessageBox.alert('訊息', '刪除項次<br>' + name + '成功');
                                            msglabel('訊息區:刪除成功');
                                            //T2Store.removeAll();
                                            T2Grid.getSelectionModel().deselectAll();
                                            T2Load();
                                            //Ext.getCmp('btnSubmit').setDisabled(true);
                                        }
                                    },
                                    failure: function (response) {
                                        Ext.MessageBox.alert('錯誤', '發生例外錯誤');
                                    }
                                });
                            }
                        });
                    }
                }
            }
        ]
    });

    // 按'新增'或'修改'時的動作
    function setFormT2(x, t) {
        viewport.down('#t1Grid').mask();
        viewport.down('#t2Grid').mask();
        viewport.down('#form').setTitle(t);
        viewport.down('#form').expand();
        var currentTab = Tabs.getActiveTab();
        currentTab.setTitle('填寫');

        var f = T2Form.getForm();
        if (x === "I") {
            isNew = true;
            var r = Ext.create('WEBAPP.model.ME_DOCD'); // /Scripts/app/model/MiUnitexch.js
            T2Form.loadRecord(r); // 建立空白model,在新增時載入T2Form以清空欄位內容
            u = f.findField("MMCODE"); // 廠商碼在新增時才可填寫
            u.setReadOnly(false);
            u.clearInvalid();

            f.findField('APPQTY').clearInvalid();

            f.findField("DOCNO2").setValue(T1LastRec.data["DOCNO"]);
            f.findField("TOWH2").setValue(T1LastRec.data["TOWH_NAME"]);

            if (f.findField("BASE_UNIT").getValue() == 'EA') {
                
                f.findField('APPQTY').decimalPrecision = 0;
            } else {
                f.findField('APPQTY').decimalPrecision = 3;
            }
        }
        else {
            u = f.findField('MMCODE');

            f.findField("DOCNO2").setValue(T2LastRec.data["DOCNO"]);
            f.findField("TOWH2").setValue(T1LastRec.data["TOWH_NAME"]);
            f.findField("MMCODE").setValue(T2LastRec.data["MMCODE"]);
            f.findField('APPQTY').setValue(T2LastRec.data["APPQTY"]);
            f.findField('APLYITEM_NOTE').setValue(T2LastRec.data["APLYITEM_NOTE"]);
            f.findField('APPQTY').clearInvalid();

            if (f.findField("BASE_UNIT").getValue() == 'EA') {
                
                f.findField('APPQTY').decimalPrecision = 0;
            } else {
                f.findField('APPQTY').decimalPrecision = 3;
            }
        }
        f.findField('x').setValue(x);
        f.findField('MMCODE').setReadOnly(false);
        f.findField('APPQTY').setReadOnly(false);
        f.findField('APLYITEM_NOTE').setReadOnly(false);
        T2Form.down('#btnMmcode').setVisible(true);
        T2Form.down('#cancel').setVisible(true);
        T2Form.down('#submit').setVisible(true);
        u.focus();
    }

    var T2Grid = Ext.create('Ext.grid.Panel', {
        //title: '核撥明細',
        store: T2Store,
        plain: true,
        loadingText: '處理中...',
        loadMask: true,
        cls: 'T1',
        dockedItems: [
            {
                dock: 'top',
                xtype: 'toolbar',
                items: [T2Tool]
            }
        ],
        selModel: {
            checkOnly: false,
            injectCheckbox: 'first',
            mode: 'MULTI'
        },
        selType: 'checkboxmodel',
        columns: [
            {
                xtype: 'rownumberer'
            },
            {
                text: "申請單號",
                dataIndex: 'DOCNO',
                width: 100
            },
            {
                text: "院內碼",
                dataIndex: 'MMCODE',
                width: 100
            },
            {
                text: "中文品名",
                dataIndex: 'MMNAME_C',
                width: 200
            },
            {
                text: "英文品名",
                dataIndex: 'MMNAME_E',
                width: 200
            },
            {
                xtype: "numbercolumn",
                text: "申請數量",
                dataIndex: 'APPQTY',
                width: 70,
                align: 'right',
                format: '0.000',
                //renderer: function (value, cellmeta, record, rowIndex, columnIndex, store) {
                //    
                //    return Number(value);
                //}
            },
            {
                text: "計量單位",
                dataIndex: 'BASE_UNIT',
                width: 70,
            },
            //{
            //    text: "核撥數量",
            //    dataIndex: 'APVQTY',
            //    width: 70,
            //    editor: {
            //        xtype: 'textfield',
            //        regexText: '只能輸入數字',
            //        regex: /^[0-9]+$/, // 用正規表示式限制可輸入內容
            //    }
            //},
            {
                text: "備註",
                dataIndex: 'APLYITEM_NOTE',
                width: 70,
            },
            {
                header: "",
                flex: 1
            }
        ],
        listeners: {
            click: {
                element: 'el',
                fn: function () {
                    if (T2Form.hidden === true) {
                        setFormVisible(false, true);
                    }
                    // grid中所有click都會觸發, 所以要判斷真的有選取到一筆record才能執行
                    if (T2LastRec != null) {

                        Ext.getCmp('btnUpdate').setDisabled(true);
                        Ext.getCmp('btnUpdate2').setDisabled(false);
                        Ext.getCmp('btnDel').setDisabled(true);
                        Ext.getCmp('btnDel2').setDisabled(false);
                        Ext.getCmp('btnAdd2').setDisabled(false);
                        Ext.getCmp('btnSubmit').setDisabled(true);
                        Ext.getCmp('btnCopy').setDisabled(true);

                        Tabs.setActiveTab('Form');

                        if (T1Set === '')
                            setFormT2a();
                    }
                }
            },
            selectionchange: function (model, records) {
                T2Rec = records.length;
                T2LastRec = records[0];
                setFormT2a();

                T2Form.getForm().findField('APPQTY').clearInvalid();
            }
        }
    });

    // 點選T2Grid一筆資料的動作
    function setFormT2a() {
        if (T2LastRec != null) {
            //viewport.down('#form').setTitle(t + T1Name);
            viewport.down('#form').expand();
            var f = T2Form.getForm();
            f.findField("DOCNO2").setValue(T2LastRec.data["DOCNO"]);
            f.findField("SEQ").setValue(T2LastRec.data["SEQ"]);
            f.findField("TOWH2").setValue(T1LastRec.data["TOWH_NAME"]);
            f.findField("MMCODE").setValue(T2LastRec.data["MMCODE"]);
            f.findField('MMNAME_C').setValue(T2LastRec.data["MMNAME_C"]);
            f.findField('MMNAME_E').setValue(T2LastRec.data["MMNAME_E"]);
            //            f.findField('M_CONTPRICE').setValue(T2LastRec.data["M_CONTPRICE"]);
            f.findField('BASE_UNIT').setValue(T2LastRec.data["BASE_UNIT"]);
            f.findField('APPQTY').setValue(T2LastRec.data["APPQTY"]);
            f.findField('APLYITEM_NOTE').setValue(T2LastRec.data["APLYITEM_NOTE"]);

            f.findField('MMCODE').setReadOnly(true);
            f.findField('APPQTY').setReadOnly(true);
            f.findField('APLYITEM_NOTE').setReadOnly(true);
            T2Form.down('#btnMmcode').setVisible(false);
            T2Form.down('#cancel').setVisible(false);
            T2Form.down('#submit').setVisible(false);

            if (f.findField("BASE_UNIT").getValue() == 'EA') {
                
                f.findField('APPQTY').decimalPrecision = 0;
            } else {
                f.findField('APPQTY').decimalPrecision = 3;
            }
            
            if (T1LastRec != null && (session['UserId'] !== T1LastRec.data.CREATE_USER || T1LastRec.data.FLOWID.split(' ')[0] != "0401")) {
                // Ext.getCmp("btnUpdate").setDisabled(true);
                //  Ext.getCmp("btnDel").setDisabled(true);
                // Ext.getCmp("btnSubmit").setDisabled(true);
                Ext.getCmp("btnAdd2").setDisabled(true);
                Ext.getCmp("btnUpdate2").setDisabled(true);
                Ext.getCmp("btnDel2").setDisabled(true);
            }
            else {
                // Ext.getCmp("btnUpdate").setDisabled(false);
                // Ext.getCmp("btnDel").setDisabled(false);
                // Ext.getCmp("btnSubmit").setDisabled(false);
                Ext.getCmp("btnAdd2").setDisabled(false);
                Ext.getCmp("btnUpdate2").setDisabled(false);
                Ext.getCmp("btnDel2").setDisabled(false);
            }
        }

    }

    function T1Cleanup() {
        T1Set = '';
        viewport.down('#t1Grid').unmask();
        viewport.down('#t2Grid').unmask();
        Ext.getCmp('eastform').collapse();
        var f = T1Form.getForm();
        f.reset();
        //f.getFields().each(function (fc) {
        //    if (fc.xtype === "displayfield" || fc.xtype === "textfield") {
        //        fc.setReadOnly(true);
        //    } else if (fc.xtype === "combo" || fc.xtype === "datefield") {
        //        fc.setReadOnly(true);
        //    }
        //});
        T1Form.down('#cancel').hide();
        T1Form.down('#submit').hide();
        f.findField('FRWH').setReadOnly(true);
        f.findField('TOWH').setReadOnly(true);
        viewport.down('#form').setTitle('瀏覽');
        var currentTab = Tabs.getActiveTab();
        currentTab.setTitle('瀏覽');
        //setFormT1a();
    }

    function setMmcode(args) {
        var f = T2Form.getForm();
        if (args.MMCODE !== '') {
            f.findField("MMCODE").setValue(args.MMCODE);
            f.findField("MMNAME_C").setValue(args.MMNAME_C);
            f.findField("MMNAME_E").setValue(args.MMNAME_E);
            //            f.findField("M_CONTPRICE").setValue(args.M_CONTPRICE);
            f.findField("BASE_UNIT").setValue(args.BASE_UNIT);
            if (f.findField("BASE_UNIT").getValue() == 'EA') {
                
                f.findField('APPQTY').decimalPrecision = 0;
            } else {
                f.findField('APPQTY').decimalPrecision = 3;
            }


        }
    }

    var mmCodeCombo = Ext.create('WEBAPP.form.MMCodeCombo', {
        name: 'MMCODE',
        fieldLabel: '院內碼',
        fieldCls: 'required',
        allowBlank: false,

        //限制一次最多顯示10筆
        limit: 10,

        //指定查詢的Controller路徑
        queryUrl: '/api/AB0022/GetMMCodeCombo',

        //查詢完會回傳的欄位
        extraFields: ['MAT_CLASS', 'BASE_UNIT'],

        //查詢時Controller固定會收到的參數
        getDefaultParams: function () {
            var f = T2Form.getForm();
            if (!f.findField("MMCODE").readOnly) {
                //tmpArray = f.findField("TOWH2").getValue().split(' ');
                return {
                    //WH_NO: tmpArray[0]
                    WH_NO: T1Form.getForm().findField('FRWH').getValue()
                };
            }
        },
        listeners: {
            select: function (c, r, i, e) {
                //選取下拉項目時，顯示回傳值
                //alert(r.get('MAT_CLASS'));
                var f = T2Form.getForm();
                if (r.get('MMCODE') !== '') {
                    f.findField("MMCODE").setValue(r.get('MMCODE'));
                    f.findField("MMNAME_C").setValue(r.get('MMNAME_C'));
                    f.findField("MMNAME_E").setValue(r.get('MMNAME_E'));
                    f.findField("BASE_UNIT").setValue(r.get('BASE_UNIT'));

                    if (f.findField("BASE_UNIT").getValue() == 'EA') {
                        
                        f.findField('APPQTY').decimalPrecision = 0;
                    } else {
                        f.findField('APPQTY').decimalPrecision = 3;
                    }
                }
            }
        }
    });

    Ext.define('Ext.overrides.form.field.Text', {
        override: 'Ext.form.field.Base',
        /**
         * Ensure all text fields have spell check disabled
         */
        inputAttrTpl: [
            'spellcheck=false'
        ],
        constructor: function () {

            this.callParent(arguments);

        }
    });

    Ext.define('MyApp.view.component.Number', {
        override: 'Ext.form.field.Number',
        forcePrecision: false,

        // changing behavior of valueToRaw with forcePrecision
        valueToRaw: function (value) {
            var me = this,
                decimalSeparator = me.decimalSeparator;

            value = me.parseValue(value); // parses value received from the field
            value = me.fixPrecision(value); // applies precision
            value = Ext.isNumber(value) ? value :
                parseFloat(String(value).replace(decimalSeparator, '.'));
            value = isNaN(value) ? '' : String(value).replace('.', decimalSeparator);

            // forcePrecision: true - retains a precision
            // forcePrecision: false - does not retain precision for whole numbers
            if (value == "") {
                return "";

            }
            else {
                return me.forcePrecision ? Ext.Number.toFixed(
                    parseFloat(value),
                    me.decimalPrecision)
                    :
                    parseFloat(
                        Ext.Number.toFixed(
                            parseFloat(value),
                            me.decimalPrecision
                        )
                    );
            }
        }
    });

    // 顯示明細/新增/修改輸入欄
    var T2Form = Ext.widget({
        xtype: 'form',
        layout: 'vbox',
        frame: false,
        hidden: true,
        cls: 'T2b',
        title: '',
        autoScroll: true,
        bodyPadding: '5 5 0',
        fieldDefaults: {
            labelAlign: 'right',
            msgTarget: 'side',
            labelWidth: 60,
            width: 300,
        },
        defaultType: 'textfield',
        items: [
            {
                fieldLabel: 'Update',
                name: 'x',
                xtype: 'hidden'
            },
            {
                fieldLabel: '項次',
                name: 'SEQ',
                xtype: 'hidden'
            },
            {
                xtype: 'displayfield',
                fieldLabel: '申請單號',
                name: 'DOCNO2',
                enforceMaxLength: true,
                maxLength: 100,
                readOnly: true,
                submitValue: true
            },
            {
                xtype: 'displayfield',
                fieldLabel: '繳回庫房',
                name: 'TOWH2',
                enforceMaxLength: true,
                maxLength: 100,
                readOnly: true,
                submitValue: true
            },
            //{
            //    xtype: 'textfield',
            //    fieldLabel: '院內碼',
            //    name: 'MMCODE',
            //    editable: false,
            //    submitValue: true,
            //    allowBlank: false,
            //    fieldCls: 'required',
            //    listeners: {
            //        render: function () {
            //            this.getEl().on('mousedown', function (e, t, eOpts) {
            //                var f = T2Form.getForm();
            //                if (!f.findField("MMCODE").readOnly) {
            //                    tmpArray = f.findField("FRWH2").getValue().split(' ');
            //                    popMmcodeForm(viewport, { MMCODE: f.findField("MMCODE").getValue(), WH_NO: tmpArray[0], E_IFPUBLIC: '0', closeCallback: setMmcode }); // E_IFPUBLIC=0 為公藥
            //                }
            //            });
            //        }
            //    }
            //},
            {
                xtype: 'container',
                layout: 'hbox',
                padding: '0 7 7 0',
                items: [
                    mmCodeCombo,
                    {
                        xtype: 'button',
                        itemId: 'btnMmcode',
                        iconCls: 'TRASearch',
                        handler: function () {
                            var f = T2Form.getForm();
                            if (!f.findField("MMCODE").readOnly) {
                                
                                tmpArray = f.findField("TOWH2").getValue().split(' ');
                                popMmcodeForm(viewport, '/api/AB0022/GetMmcode', { MMCODE: f.findField("MMCODE").getValue(), WH_NO: tmpArray[0] }, setMmcode);
                            }
                        }
                    },

                ]
            },
            {
                xtype: 'displayfield',
                fieldLabel: '中文品名',
                name: 'MMNAME_C',
                enforceMaxLength: true,
                maxLength: 100,
                readOnly: true,
                submitValue: true
            },
            {
                xtype: 'displayfield',
                fieldLabel: '英文品名',
                name: 'MMNAME_E',
                enforceMaxLength: true,
                maxLength: 100,
                readOnly: true,
                submitValue: true
            },
            //{
            //    xtype: 'displayfield',
            //    fieldLabel: '進貨單價',
            //    name: 'M_CONTPRICE',
            //    enforceMaxLength: true,
            //    maxLength: 100,
            //    readOnly: true,
            //    submitValue: true
            //},
            {
                xtype: 'displayfield',
                fieldLabel: '計量單位',
                name: 'BASE_UNIT',
                enforceMaxLength: true,
                maxLength: 100,
                readOnly: true,
                submitValue: true
            },
            //{
            //    xtyle: 'textfield',
            //    regex: /^[0-9]{1,11}(\.[0-9]{0,3})?$/g,
            //    fieldLabel: '申請數量',
            //    name: 'APPQTY',
            //    submitValue: true,
            //    allowBlank: false,
            //    fieldCls: 'required',
            //}
            {
                xtype: 'numberfield',
                fieldLabel: '申請數量',
                name: 'APPQTY',
                //regex: /^[0-9]{1,11}(\.[0-9]{0,3})?$/g,
                enforceMaxLength: true,
                maxLength: 15,
                submitValue: true,
                allowBlank: false,
                fieldCls: 'required',
                hideTrigger: true,
                minValue: 0,
                maxValue: 99999999999.999,
                decimalPrecision: 3,
                allowDecimals: true,
                forcePrecision: true,
                listeners: {
                    change: function (self, newValue, oldValue, eOpts) {
                        
                    }
                }
            },
            {
                xtype: 'textareafield',
                fieldLabel: '備註',
                name: 'APLYITEM_NOTE',
                labelAlign: 'right',
                readOnly: true,
                allowBlank: true,
                maxLength: 180,
            }
            //{
            //    xtype: 'textfield',
            //    fieldLabel: '備註',
            //    name: 'APLYITEM_NOTE',
            //    enforceMaxLength: true,
            //    maxLength: 8,
            //    submitValue: true
            //},
        ],
        buttons: [
            {
                itemId: 'submit', text: '儲存', hidden: true,
                handler: function () {
                    if (this.up('form').getForm().isValid()) { // 檢查T1Form填寫資料是否符合規則(必填欄位都有填、輸入內容有符合正規表示式等)
                        if (this.up('form').getForm().findField('APPQTY').getValue() == 0)
                            Ext.Msg.alert('提醒', '申請數量不得為0');
                        else {
                            var confirmSubmit = viewport.down('#form').title.substring(0, 2);
                            Ext.MessageBox.confirm(confirmSubmit, '是否確定' + confirmSubmit + '?', function (btn, text) {
                                if (btn === 'yes') {
                                    
                                    T2Submit();
                                }
                            });
                        }

                        //var confirmSubmit = viewport.down('#form').title.substring(0, 2);
                        //Ext.MessageBox.confirm(confirmSubmit, '是否確定' + confirmSubmit + '?', function (btn, text) {
                        //    if (btn === 'yes') {
                        //        T2Submit();
                        //        T1Set = '';
                        //    }
                        //});
                    }
                    else
                        Ext.Msg.alert('提醒', '輸入資料格式有誤');
                }
            },
            {
                itemId: 'cancel', text: '取消', hidden: true, handler: T2Cleanup
            }
        ]
    });

    function T2Cleanup() {
        T1Set = '';
        viewport.down('#t1Grid').unmask();
        viewport.down('#t2Grid').unmask();
        Ext.getCmp('eastform').collapse();
        var f = T2Form.getForm();
        f.reset();
        //f.getFields().each(function (fc) {
        //    if (fc.xtype === "displayfield" || fc.xtype === "textfield") {
        //        fc.setReadOnly(true);
        //    } else if (fc.xtype === "combo" || fc.xtype === "datefield") {
        //        fc.setReadOnly(true);
        //    }
        //});
        T1Form.down('#cancel').hide();
        T1Form.down('#submit').hide();
        viewport.down('#form').setTitle('瀏覽');
        var currentTab = Tabs.getActiveTab();
        currentTab.setTitle('瀏覽');
        //setFormT1a();
    }

    function T2Submit() {
        
        var f = T2Form.getForm();
        if (f.isValid()) {
            var myMask = new Ext.LoadMask(viewport, { msg: '處理中...' });
            myMask.show();
            f.submit({
                url: T1Set,
                success: function (form, action) {
                    myMask.hide();
                    var f2 = T2Form.getForm();
                    var r = f2.getRecord();
                    switch (f2.findField("x").getValue()) {
                        case "I":
                            //var v = action.result.etts[0];
                            //T1Query.getForm().findField('P0').setValue(v.DN);
                            T1Load();
                            T2Load();
                            msglabel('訊息區:資料新增成功');
                            break;
                        case "U":
                            T2Load();
                            msglabel('訊息區:資料更新成功');
                            break;
                    }
                    
                    var isWexpid = action.result.msg;
                    if (isWexpid == 'Y') {
                        showExpWindow(T1LastRec.data.DOCNO, 'O', viewport);
                    }

                    T2Cleanup();
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
                            Ext.Msg.alert('失敗', action.result.msg);
                            break;
                    }
                }
            });
        }
    }

    var setFormVisible = function (t1Form, t2Form) {
        T1Form.setVisible(t1Form);
        T2Form.setVisible(t2Form);
    }

    var Tabs = Ext.widget('tabpanel', {
        plain: true,
        border: false,
        resizeTabs: true,
        layout: 'fit',
        defaults: {
            layout: 'fit'
        },
        items: [{
            itemId: 'Query',
            title: '查詢',
            items: [T1Query]
        }, {
            itemId: 'Form',
            title: '瀏覽',
            items: [T1Form, T2Form]
        }]
    });

    var viewport = Ext.create('Ext.Viewport', {
        renderTo: body,
        layout: {
            type: 'border',
            padding: 0
        },
        defaults: {
            split: true  //可以調整大小
        },
        items: [
            {
                itemId: 't1top',
                region: 'center',
                layout: 'border',
                collapsible: false,
                title: '',
                border: false,
                items: [
                    {
                        itemId: 't1Grid',
                        region: 'north',
                        layout: 'fit',
                        collapsible: false,
                        title: '',
                        border: false,
                        height: '50%',
                        split: true,
                        items: [T1Grid]
                    },
                    {
                        itemId: 't2Grid',
                        region: 'center',
                        layout: 'fit',
                        collapsible: false,
                        title: '',
                        height: '50%',
                        split: true,
                        items: [T2Grid]
                    }
                ]
            },
            {
                itemId: 'form',
                id: 'eastform',
                region: 'east',
                collapsible: true,
                floatable: true,
                width: 350,
                title: '瀏覽',
                border: false,
                layout: {
                    type: 'fit',
                    padding: 5,
                    align: 'stretch'
                },
                items: [Tabs]
            }
        ]
    });
    //UserInfoLoad();
    FrwhLoad();
    QueryFrwhLoad();
    QueryAppidLoad();

    Ext.getCmp('P5').setDisabled(true);

    Ext.getCmp('btnUpdate').setDisabled(true);
    Ext.getCmp('btnDel').setDisabled(true);
    Ext.getCmp('btnSubmit').setDisabled(true);
    Ext.getCmp('btnCopy').setDisabled(true);

    Ext.getCmp('btnAdd2').setDisabled(true);
    Ext.getCmp('btnUpdate2').setDisabled(true);
    Ext.getCmp('btnDel2').setDisabled(true);


});