Ext.Loader.setConfig({
    enabled: true,
    paths: {
        'WEBAPP': '/Scripts/app'
    }
});

Ext.require(['WEBAPP.utils.Common']);

Ext.onReady(function () {
    var T1Name = "核撥作業";
    var T1LastRec;
    var T2LastRec;
    var tmp, arrow, VvRadio;
    var chkWexpidGet = '/api/AA0015/ChkWexpid';
    var chkWexpidDetailGet = '/api/AA0015/ChkWexpidDetail';

    var viewModel = Ext.create('WEBAPP.store.AA.AA0015VM');
    var T1Store = viewModel.getStore('ME_DOCM');
    function T1Load() {
        T1Store.getProxy().setExtraParam("p1", T1Query.getForm().findField('P1').getValue());
        T1Store.getProxy().setExtraParam("p2", T1Query.getForm().findField('P2').getValue());
        T1Store.getProxy().setExtraParam("p3", T1Query.getForm().findField('P3').getValue());
        //T1Store.getProxy().setExtraParam("p4", T1Query.getForm().findField('P4').getValue());
        T1Store.getProxy().setExtraParam("p5", T1Query.getForm().findField('P5').getValue());
        T1Store.getProxy().setExtraParam("p6", T1Query.getForm().findField('P6').getValue());
        T1Store.getProxy().setExtraParam("d0", T1Query.getForm().findField('D0').getValue());
        T1Store.getProxy().setExtraParam("d1", T1Query.getForm().findField('D1').getValue());
        T1Store.getProxy().setExtraParam("FLOWID", T1Query.getForm().findField('FLOWID').getValue());
        T1Store.getProxy().setExtraParam("p8", T1Query.getForm().findField('P8').getValue());
        //T1Store.getProxy().setExtraParam("DOCTYPE", 'MR');
        //T1Store.getProxy().setExtraParam("FLOWID", '0102'); // 藥庫核撥中
        T1Tool.moveFirst();
    }

    var T2Store = viewModel.getStore('ME_DOCD');
    function T2Load() {
        if (T1LastRec != null && T1LastRec.data["DOCNO"] !== '') {
            var tmpArray = T1LastRec.data["FRWH_NAME"].split(' ');

            T2Store.getProxy().setExtraParam("p0", T1LastRec.data["DOCNO"]);
            T2Store.getProxy().setExtraParam("p1", tmpArray[0]);
            T2Store.getProxy().setExtraParam("p8", T1Query.getForm().findField('P8').getValue());
            T2Store.load({
                params: {
                    start: 0
                }
                //,
                //callback: function () {
                //    if (T1LastRec.data['DOCNO'] != '*') {
                //        if (T2Store.getCount() > 0)
                //            T2Grid.editingPlugin.startEdit(0, 13);
                //    }
                //}
            });
        }
        else
            T2Store.removeAll();
    }

    function getTodayDate() {
        var fullDate = new Date();
        var yyy = fullDate.getFullYear() - 1911;
        var MM = (fullDate.getMonth() + 1) >= 10 ? (fullDate.getMonth() + 1) : ("0" + (fullDate.getMonth() + 1));
        var dd = fullDate.getDate() < 10 ? ("0" + fullDate.getDate()) : fullDate.getDate();
        var today = yyy + MM + dd;
        return today;
    }

    function getToday() {
        var today = new Date();
        today.setDate(today.getDate());
        return today
    }
    var arrP8 = ["Y","N","D"];

    var st_Postid = Ext.create('Ext.data.Store', {
        proxy: {
            type: 'ajax',
            actionMethods: {
                read: 'POST' // by default GET
            },
            url: '/api/AA0015/GetPostidCombo',
            reader: {
                type: 'json',
                rootProperty: 'etts'
            }
        },
        autoLoad: true
    });
    var st_Appdept = Ext.create('Ext.data.Store', {
        proxy: {
            type: 'ajax',
            actionMethods: {
                read: 'POST' // by default GET
            },
            url: '/api/AA0015/GetAppdeptCombo',
            reader: {
                type: 'json',
                rootProperty: 'etts'
            }
        },
        autoLoad: true
    });
    var st_Frwh = Ext.create('Ext.data.Store', {
        proxy: {
            type: 'ajax',
            actionMethods: {
                read: 'POST' // by default GET
            },
            url: '/api/AA0015/GetFrwhCombo',
            reader: {
                type: 'json',
                rootProperty: 'etts'
            }
        },
        autoLoad: true,
        listeners: {
            load: function (store, records, successful, eOpts) {
                if (records.length > 0) {
                    T1Query.getForm().findField('P5').setValue(records[0].data.VALUE);
                }
            }
        }
    });
    var st_Towh = Ext.create('Ext.data.Store', {
        proxy: {
            type: 'ajax',
            actionMethods: {
                read: 'POST' // by default GET
            },
            url: '/api/AA0015/GetTowhCombo',
            reader: {
                type: 'json',
                rootProperty: 'etts'
            }
        },
        autoLoad: true
    });

    var st_docno = Ext.create('Ext.data.Store', {
        proxy: {
            type: 'ajax',
            actionMethods: {
                read: 'POST' // by default GET
            },
            url: '/api/AA0015/GetDocnoCombo',
            reader: {
                type: 'json',
                rootProperty: 'etts'
            },
        },
        autoLoad: true
    });
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
                fieldLabel: '單據號碼',
                name: 'P1',
                id: 'P1',
                enforceMaxLength: true,
                maxLength: 21,
                width: 300,
                padding: '0 4 0 4'
            },
            {
                xtype: 'datefield',
                fieldLabel: '申請日期',
                name: 'D0',
                id: 'D0',
                regex: /\d{7,7}/,
                labelWidth: 90,
                enforceMaxLength: true, // 限制可輸入最大長度
                maxLength: 7, 
                padding: '0 4 0 4',
                value: getToday()
            },
            {
                xtype: 'datefield',
                fieldLabel: '至',
                labelWidth: '10px',
                name: 'D1',
                id: 'D1',
                labelSeparator: '',
                regex: /\d{7,7}/,
                labelWidth: 90,
                enforceMaxLength: true,
                maxLength: 7, 
                padding: '0 4 0 4'
            },
            {
                xtype: 'textfield',
                fieldLabel: '申請人員',
                name: 'P2',
                id: 'P2',
                enforceMaxLength: true,
                maxLength: 8,
                width: 300,
                padding: '0 4 0 4'
            },
            {
                xtype: 'combo',
                fieldLabel: '申請部門',
                name: 'P3',
                id: 'P3',
                store: st_Appdept,
                queryMode: 'local',
                displayField: 'COMBITEM',
                valueField: 'VALUE',
                tpl: '<tpl for="."><div class="x-boundlist-item" style="height:auto;">{COMBITEM}&nbsp;</div></tpl>',
                padding: '0 4 0 4'
            },
            {
                xtype: 'combo',
                fieldLabel: '核撥庫房',
                name: 'P5',
                id: 'P5',
                store: st_Frwh,
                queryMode: 'local',
                displayField: 'COMBITEM',
                valueField: 'VALUE',
                tpl: '<tpl for="."><div class="x-boundlist-item" style="height:auto;">{COMBITEM}&nbsp;</div></tpl>',
                padding: '0 4 0 4'
            },
            {
                xtype: 'combo',
                fieldLabel: '申請庫房',
                name: 'P6',
                id: 'P6',
                store: st_Towh,
                queryMode: 'local',
                displayField: 'COMBITEM',
                valueField: 'VALUE',
                tpl: '<tpl for="."><div class="x-boundlist-item" style="height:auto;">{COMBITEM}&nbsp;</div></tpl>',
                padding: '0 4 0 4'
            }, {
                xtype: 'combo',
                fieldLabel: '過帳狀態',
                name: 'P8',
                id: 'P8',
                store: st_Postid,
                queryMode: 'local',
                multiSelect: true,
                tpl: '<tpl for="."><div class="x-boundlist-item" style="height:auto;">{TEXT}&nbsp;</div></tpl>',
                displayField: 'TEXT',
                valueField: 'VALUE'
            },
            {
                xtype: 'checkboxgroup',
                fieldLabel: '單據狀態',
                name: 'FLOWID',
                columns: 1,
                vertical: true,
                items: [
                    { boxLabel: '手動申請-核撥中', name: 'rb', inputValue: '0102', checked: true },
                    { boxLabel: '公藥申請-核撥中', name: 'rb', inputValue: '0602', checked: true },
                    { boxLabel: '手動申請-點收中', name: 'rb', inputValue: '0103' },
                    { boxLabel: '公藥申請-點收中', name: 'rb', inputValue: '0603' },
                    { boxLabel: '手動申請-沖帳中', name: 'rb', inputValue: '0104', checked: true },
                    { boxLabel: '公藥申請-沖帳中', name: 'rb', inputValue: '0604', checked: true },
                    { boxLabel: '手動申請-結案', name: 'rb', inputValue: '0199' },
                    { boxLabel: '公藥申請-結案', name: 'rb', inputValue: '0699' },
                ]
            }
        ],
        buttons: [
            {
                itemId: 'query',
                text: '查詢',
                handler: function () {
                    T1Load();
                    Ext.getCmp('eastform').collapse();
                    T2Store.removeAll();
                }
            },
            {
                itemId: 'clean',
                text: '清除',
                handler: function () {
                    var f = this.up('form').getForm();
                    f.reset();
                    f.findField('P1').focus(); // 進入畫面時輸入游標預設在P0
                    T1Query.getForm().findField('P8').setValue(arrP8);
                    T2Store.removeAll();
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
            //{
            //    text: '查詢條件',
            //    handler: function () {
            //        Ext.getCmp('eastform').expand();
            //    }
            //},
            {
                text: '核撥',
                id: 'btnSubmit',
                name: 'btnSubmit',
                handler: function () {
                    //if (T1LastRec == null || T1LastRec.data["DOCNO"] == '') {
                    //    Ext.MessageBox.alert('錯誤', '尚未選取單據號碼');
                    //    return;
                    //}
                    var selection = T1Grid.getSelection();
                    if (selection.length) {
                        chkWexpid(selection);
                    }
                    else {
                        Ext.MessageBox.alert('錯誤', '尚未選取單據號碼');
                        return;
                    }
                }
            }, {
                text: '完成沖帳',
                id: 'btnSubmit2',
                name: 'btnSubmit2',
                handler: function () {
                    var selection = T1Grid.getSelection();
                    if (selection.length) {
                        let name = '';
                        let docno = '';
                        $.map(selection, function (item, key) {
                            name += '「' + item.get('DOCNO') + '」<br>';
                            docno += item.get('DOCNO') + ',';
                        })
                        
                        Ext.MessageBox.confirm('完成沖帳', '確認是否完成沖帳,單據號碼<br>' + name, function (btn, text) {
                            if (btn === 'yes') {

                                Ext.Ajax.request({
                                    url: '/api/AA0015/UpdateStatusBySP2',
                                    method: reqVal_p,
                                    params: {
                                        DOCNO: docno
                                    },
                                    //async: true,
                                    success: function (response) {
                                        var data = Ext.decode(response.responseText);
                                        if (data.success) {
                                            msglabel('核撥成功');
                                            T2Store.removeAll();
                                            T1Grid.getSelectionModel().deselectAll();
                                            T1Load();
                                            Ext.getCmp('btnSubmit2').setDisabled(true);
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
                itemId: 'btnExp',
                id: 'btnExp',
                text: '效期',
                disabled: true,
                handler: function () {
                    showExpWindow(T1LastRec.data["DOCNO"], 'O', viewport);
                }
            },
            {
                xtype: 'button',
                text: '至手動申請',
                handler: function () {
                    parent.link2('../../../Form/Index/AB0010', '手動申請(AB0010)');
                }
            }, {
                xtype: 'button',
                text: '待核撥列印',
                id: 'T1print_non',
                name: 'T1print_non',
                handler: function () {
                    reportUrl = '/Report/A/AA0015_NON.aspx';
                    //showReport();
                    showWin3();
                }
            },
            {
                xtype: 'button',
                text: '已核撥列印',
                id: 'T1print',
                name: 'T1print',
                handler: function () {
                    reportUrl = '/Report/A/AA0015.aspx';
                    //showReport();
                    showWin3();
                }
            },
            {
                xtype: 'button',
                text: '退回',
                id: 'return',
                name: 'return',
                disabled:true,
                handler: function () {
                    if (T1LastRec == null) {
                        Ext.Msg.alert('提醒', '請選擇要退回的申請單');
                        return;
                    }
                    T4Form.getForm().findField('DOCNO').setValue(T1LastRec.data.DOCNO);

                    returnWindow.show();

                }
            }
        ]
    });
    Ext.getCmp('btnSubmit').setDisabled(true);
    Ext.getCmp('btnSubmit2').setDisabled(true);

    var T1Grid = Ext.create('Ext.grid.Panel', {
        //title: T1Name,
        store: T1Store,
        plain: true,
        loadingText: '處理中...',
        loadMask: true,
        cls: 'T1',
        dockedItems: [
            //{
            //    dock: 'top',
            //    xtype: 'toolbar',
            //    items: [T1Query]
            //},
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
            }, {
                text: "單據號碼",
                dataIndex: 'DOCNO',
                width: 100
            }, {
                xtype: 'hidden',
                text: "狀態",
                dataIndex: 'FLOWID',
                width: 120
            }, {
                text: "申請人員",
                dataIndex: 'APPID',
                width: 160
            }, {
                xtype: 'hidden',
                text: "申請人員",
                dataIndex: 'APP_NAME',
                width: 160
            }, {
                xtype: 'hidden',
                text: "申請部門",
                dataIndex: 'APPDEPT_NAME',
                width: 150
            }, {
                text: "申請日期",
                dataIndex: 'APPTIME',
                width: 120
            }, {
                text: "申請庫房",
                dataIndex: 'TOWH_NAME',
                width: 150
            }, {
                text: "核撥庫房",
                dataIndex: 'FRWH_NAME',
                width: 150
            }, {
                text: "單據狀態",
                dataIndex: 'FLOWID',
                width: 150
            }, {
                header: "",
                flex: 1
            }
        ],
        listeners: {
            click: {
                element: 'el',
                fn: function () {
                    // grid中所有click都會觸發, 所以要判斷真的有選取到一筆record才能執行
                    if (T1LastRec != null) {
                        T2Load();
                        tmpArray1 = T1LastRec.data["FLOWID"].split(' ');
                        var tmpArray;
                        if (tmpArray1[0] == '0104' || tmpArray1[0] == '0604') {
                            //Ext.getCmp('detailSubmit').setDisabled(false);
                        }
                        else {

                            //Ext.getCmp('detailSubmit').setDisabled(true);
                        }
                    }
                }
            },
            selectionchange: function (model, records) {
                Ext.getCmp('btnSubmit').setDisabled(false);
                Ext.getCmp('btnExp').setDisabled(false);
                T1Rec = records.length;
                T1LastRec = records[0];
                if (T1LastRec != null) {
                    tmpArray1 = T1LastRec.data["FLOWID"].split(' ');
                    var tmpArray;
                    if (tmpArray1[0] == '0104' || tmpArray1[0] == '0604') {
                        Ext.getCmp('btnSubmit2').setDisabled(false);
                    }
                    else {

                        Ext.getCmp('btnSubmit2').setDisabled(true);
                    }
                    if (tmpArray1[0] == '0102' || tmpArray1[0] == '0602') {
                        Ext.getCmp('btnSubmit').setDisabled(false);
                        Ext.getCmp('return').setDisabled(false);
                    }
                    else {

                        Ext.getCmp('btnSubmit').setDisabled(true);
                        Ext.getCmp('return').setDisabled(true);
                    }
                    T3Form.getForm().findField("T3P0").setValue(T1LastRec.data["DOCNO"]);
                    T3Form.getForm().findField("T3P6").setValue(T1LastRec.data["FRWH_NAME"].split(' ')[0]);
                }
                //Ext.getCmp('detailSubmit').setDisabled(true);
                T2Load();
                
                if (T1Rec > 1) {
                    Ext.getCmp('return').setDisabled(true);
                }
                if (T1Rec == 0) {
                    Ext.getCmp('btnSubmit').setDisabled(true);
                    Ext.getCmp('return').setDisabled(true);
                    Ext.getCmp('btnExp').setDisabled(true);
                }
                
            }
        }
    });

    var T2RowEditing = Ext.create('Ext.grid.plugin.RowEditing', {
        //clicksToMoveEditor: 1,
        clicksToEdit: 1,
        autoCancel: false,
        saveBtnText: '更新',
        cancelBtnText: '取消',
        errorsText: '錯誤訊息',
        dirtyText: '請按更新以修改資料或取消變更',
        listeners: {
            beforeedit: function (editor, context, eOpts) {
                //if (Ext.getCmp('RLNO').getValue() == 'PGRM') {
                //if (Ext.getCmp('viewMode').disabled) {
                //    return false;  // 取消row editing模式
                //} else {
                    //var grid = Ext.ComponentQuery.query('grid#grid_contact');
                //    var grid = Ext.ComponentQuery.query('grid')[2];
                    //var col = grid.getView().getHeaderCt().getHeaderAtIndex(4);
                    //var cols = grid.getView().getHeaderCt().getGridColumns();
                    //Ext.each(cols, function (col) {
                    //    if (col.text == "原料記號") {
                    //        col.setEditor(null);
                    //    }
                    //});

                    //if (context.column.dataIndex == 'PARAU')
                    //col.setEditor(null);
                //}
            },
            edit: function (editor, context, eOpts) {
                //var grid = Ext.ComponentQuery.query('#grid_contact')[0];
                //var store = grid.getView().getStore();

                var data = { item: [] };
                data.item.push(context.record.data);

                //alert(context.record.data['NAME_LAST']);
                Ext.Ajax.request({
                    url: '/api/AA0015/UpdateMeDocd',
                    method: reqVal_p,
                    //params: {
                    //    VID5: context.record.data['VID5']
                    //},
                    //async: true,
                    success: function (response) {
                        var data = Ext.decode(response.responseText);
                        if (data.success) {
                        }
                    },
                    failure: function (response) {
                        Ext.MessageBox.alert('錯誤', '發生例外錯誤');
                    },
                    jsonData: data
                });
            },
            canceledit: function (editor, context, eOpts) {
            }
        }
    });

    var T2CellEditing = Ext.create('Ext.grid.plugin.CellEditing', {
        clicksToEdit: 1,
        listeners: {
            beforeedit: function (plugin, context) {
                tmpArray1 = T1LastRec.data["FLOWID"].split(' ');
                var tmpArray;
                var flag = false;
                if (context.record.get('POSTID') != null) {
                    tmpArray = context.record.get('POSTID').split(' ');
                    flag = true;
                }

                //if (tmpArray1[0] == '0104' || tmpArray1[0] == '0604') {
                //    Ext.getCmp('detailSubmit').setText('完成沖帳');
                //    Ext.getCmp('detailSubmit').setDisabled(false);
                //}

                //else if (tmpArray1[0] == '0102' || tmpArray1[0] == '0602') {
                //    Ext.getCmp('detailSubmit').setText('完成揀料');
                //    Ext.getCmp('detailSubmit').setDisabled(false);
                //}
                //else
                //    Ext.getCmp('detailSubmit').setDisabled(true);
                
                if (tmpArray1[0] == '0103' || tmpArray1[0] == '0603' || tmpArray1[0] == '0199' || tmpArray1[0] == '0699')
                    return false;
                else {
                    if (flag && (tmpArray[0] == 'C'))
                        return false;
                    else {
                        if (context.record.get('APVQTY') != '') 
                            tmp = context.record.get('APVQTY'); // 先暫存原值
                        //context.record.set('APVQTY', '');   // 數量預設清空
                        if (tmpArray1[0] == '0102' || tmpArray1[0] == '0602')
                        {
                            if (tmpArray[0] == '待核撥')
                                return true;
                            else
                                return false;
                        }
                        else if (tmpArray1[0] == '0104' || tmpArray1[0] == '0604')
                        {
                            if (tmpArray[0] == 'D' && context.record.get('ONWAY_QTY') != 0) // POSTID=D且在途量>0,可修改核撥量
                                return true;
                            else
                                return false;
                        }
                        else
                            return false;
                    }
                }
            },
            validateedit: function (editor, context, eOpts) {
                if (context.record.get('APVQTY') == '')
                    context.record.set('APVQTY', tmp);  // 如未修改,則把原值寫回
            },
            edit: function (editor, context, eOpts) {
                var modifiedRec = context.store.getModifiedRecords();
                var r = context.record;
                var docno = r.get('DOCNO');
                var mmcode = (context.field === 'MMCODE') ? context.value : r.get('MMCODE');
                var apvqty = (context.field === 'APVQTY') ? context.value : r.get('APVQTY');

                Ext.each(modifiedRec, function (item) {
                    if (item.crudState === 'U') {
                        Ext.Ajax.request({
                            url: '/api/AA0015/UpdateMeDocd',
                            method: reqVal_p,
                            params: {
                                DOCNO: docno,
                                SEQ: r.get('SEQ'),
                                APVQTY: apvqty
                            },
                            success: function (response) {
                                var data = Ext.decode(response.responseText);
                                if (data.success) {
                                    item.commit();
                                    msglabel('資料更新成功');

                                    if (arrow == 'DOWN')
                                        editor.startEdit(context.rowIdx + 1, 13);
                                    else if (arrow == 'UP')
                                        editor.startEdit(context.rowIdx - 1, 13);
                                }
                                else {
                                    item.reject();
                                    Ext.Msg.alert('提醒', data.msg, function () {
                                        editor.startEdit(context.rowIdx, 13);
                                    });

                                }
                            },
                            failure: function (response) {
                                Ext.MessageBox.alert('錯誤', '發生例外錯誤');
                            },
                            //jsonData: data
                        });
                    }
                });
            },
            canceledit: function (editor, context, eOpts) {
                //alert('onCancelEdit');
            }
        }
    });



    var T2Tool = Ext.create('Ext.PagingToolbar', {
        store: T2Store,
        displayInfo: true,
        border: false,
        plain: true,
        displayMsg: '顯示{0} - {1}筆,共{2}筆 　　<font color="red">*院內碼紅字為效期管理</font>',
        buttons: [
            {
                itemId: 'detailSubmit',
                text: '完成揀料',
                id: 'detailSubmit',
                name: 'detailSubmit',
                handler: function () {
                    var selection = T2Grid.getSelection();
                    if (selection.length) {
                        let seq = '';
                        let docno = '';
                        //selection.map(item => {
                        //    seq += item.get('SEQ') + ',';
                        //    docno += item.get('DOCNO') + ',';
                        //});
                        $.map(selection, function (item, key) {
                            seq += item.get('SEQ') + ',';
                            docno += item.get('DOCNO') + ',';
                        })

                        chkWexpidDetail(docno, seq);
                    }
                    else {
                        Ext.MessageBox.alert('錯誤', '尚未選取品項');
                        return;
                    }
                }
            }, {
                itemId: 'detailCancel',
                text: '取消揀料',
                id: 'detailCancel',
                name: 'detailCancel',
                handler: function () {
                    var selection = T2Grid.getSelection();
                    if (selection.length) {
                        let seq = '';
                        let docno = '';
                        //selection.map(item => {
                        //    seq += item.get('SEQ') + ',';
                        //    docno += item.get('DOCNO') + ',';
                        //});
                        $.map(selection, function (item, key) {
                            seq += item.get('SEQ') + ',';
                            docno += item.get('DOCNO') + ',';
                        })

                        Ext.MessageBox.confirm('取消揀料', '確認是否送出?', function (btn, text) {
                            if (btn === 'yes') {
                                Ext.Ajax.request({
                                    url: '/api/AA0015/CancelPostidBySP',
                                    method: reqVal_p,
                                    params: {
                                        DOCNO: docno,
                                        SEQ: seq
                                    },
                                    //async: true,
                                    success: function (response) {
                                        var data = Ext.decode(response.responseText);
                                        if (data.success) {
                                            //T1Load();
                                            //T1LastRec = null;
                                            T2Load();
                                            Ext.getCmp('detailCancel').setDisabled(true);
                                            msglabel('取消揀料成功');
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
            }
            //{
            //    itemId: 'export',
            //    text: '匯出',
            //    id: 'export',
            //    name: 'export',
            //    handler: function () {

            //        var p0 = T1LastRec.get('DOCNO');
            //        var tmpArray = T1LastRec.data["FRWH_NAME"].split(' ');
            //        var p1 = tmpArray[0];
            //        var p = new Array();

            //        p.push({ name: 'P0', value: p0 });
            //        p.push({ name: 'P1', value: p1 });
            //        PostForm('../../../api/AA0015/Excel', p);
            //    }
            //},
            //{
            //    xtype: 'button',
            //    text: '未核撥列印',
            //    id: 'print_non',
            //    name: 'print_non',
            //    handler: function () {
            //        reportUrl = '/Report/A/AA0015_NON.aspx';
            //        //showReport();
            //        showWin3();
            //    }
            //},
            //{
            //    xtype: 'button',
            //    text: '已核撥列印',
            //    id: 'print',
            //    name: 'print',
            //    handler: function () {
            //        reportUrl = '/Report/A/AA0015.aspx';
            //        //showReport();
            //        showWin3();
            //    }
            //}
        ]
    });

    // 查詢條件-列印
    var T3Form = Ext.widget({
        xtype: 'form',
        layout: 'form',
        frame: false,
        cls: 'T1b',
        title: '',
        bodyPadding: '5 5 0',
        fieldDefaults: {
            msgTarget: 'side',
            labelWidth: 90
        },
        autoScroll: true,
        items: [
            {
                xtype: 'combo',
                fieldLabel: '單據號碼',
                labelAlign: 'right',
                name: 'T3P0',
                id: 'T3P0',
                store: st_docno,
                queryMode: 'local',
                tpl: '<tpl for="."><div class="x-boundlist-item" style="height:auto;">{COMBITEM}&nbsp;</div></tpl>',
                displayField: 'COMBITEM',
                valueField: 'VALUE'
            }, {
                xtype: 'combo',
                fieldLabel: '至',
                labelAlign: 'right',
                labelWidth: '10px',
                name: 'T3P1',
                id: 'T3P1',
                labelSeparator: '',
                store: st_docno,
                queryMode: 'local',
                displayField: 'TEXT',
                valueField: 'VALUE'
            },{
                xtype: 'datefield',
                fieldLabel: '申請日期',
                labelAlign: 'right',
                name: 'T3P2',
                id: 'T3P2',
                vtype: 'dateRange',
                dateRange: { end: 'T3P3' },
                padding: '0 4 0 4',
                value: getToday()
            }, {
                xtype: 'datefield',
                fieldLabel: '至',
                labelAlign: 'right',
                labelWidth: '10px',
                name: 'T3P3',
                id: 'T3P3',
                labelSeparator: '',
                vtype: 'dateRange',
                dateRange: { begin: 'T3P2' },
                padding: '0 4 0 4'
            }, 
            {
                xtype: 'combo',
                fieldLabel: '申請庫房',
                labelAlign: 'right',
                name: 'T3P4',
                id: 'T3P4',
                store: st_Towh,
                queryMode: 'local',
                displayField: 'COMBITEM',
                valueField: 'VALUE',
                forceSelection: true,
                tpl: '<tpl for="."><div class="x-boundlist-item" style="height:auto;">{COMBITEM}&nbsp;</div></tpl>',
                padding: '0 4 0 4'
            },
            {
                xtype: 'combo',
                fieldLabel: '核撥庫房',
                labelAlign: 'right',
                name: 'T3P6',
                id: 'T3P6',
                store: st_Frwh,
                queryMode: 'local',
                displayField: 'COMBITEM',
                valueField: 'VALUE',
                forceSelection: true,
                tpl: '<tpl for="."><div class="x-boundlist-item" style="height:auto;">{COMBITEM}&nbsp;</div></tpl>',
                padding: '0 4 0 4'
            },
            {
                xtype: 'radiogroup',
                fieldLabel: '排序',
                labelAlign: 'right',
                name: 'T3P5',
                items: [
                    { boxLabel: '儲位', name: 'T3P51',  inputValue: '1', checked: true },
                    { boxLabel: '院內碼', name: 'T3P51', inputValue: '2' }
                ],
                padding: '0 4 0 4',
            }
        ],
        buttons: [{
            itemId: 'T3print', text: '列印', handler: function () {
                VvRadio = '1';
                VvRadio = T3Form.getForm().findField('T3P5').getChecked()[0].inputValue;
                showReportT3();
            }
        },
        {
            itemId: 'T3cancel', text: '取消', handler: hideWin3
        }
        ]
    });

    function chkDefSort() {
        Ext.Ajax.request({
            url: '/api/AA0015/ChkDefSort',
            method: reqVal_p,
            params: {
                wh_no: T3Form.getForm().findField("T3P6").getValue()
            },
            //async: true,
            success: function (response) {
                var data = Ext.decode(response.responseText);
                if (data.success) {
                    if (data.msg == '0') // 核撥庫房為藥局,則排序預設為院內碼
                        T3Form.getForm().findField('T3P5').setValue({ T3P51: '2' });
                    else
                        T3Form.getForm().findField('T3P5').setValue({ T3P51: '1' });
                }
                else
                    Ext.MessageBox.alert('錯誤', data.msg);
            },
            failure: function (response) {
                Ext.MessageBox.alert('錯誤', '發生例外錯誤');
            }
        });

        
    };
    
    var win3;
    var winActWidth3 = 300;
    var winActHeight3 = 300;
    if (!win3) {
        win3 = Ext.widget('window', {
            title: '列印',
            closeAction: 'hide',
            width: winActWidth3,
            height: winActHeight3,
            layout: 'fit',
            resizable: true,
            modal: true,
            constrain: true,
            items: T3Form,
            listeners: {
                move: function (xwin, x, y, eOpts) {
                    xwin.setWidth((viewport.width - winActWidth3 > 0) ? winActWidth3 : viewport.width - 36);
                    xwin.setHeight((viewport.height - winActHeight3 > 0) ? winActHeight3 : viewport.height - 36);
                },
                resize: function (xwin, width, height) {
                    winActWidth3 = width;
                    winActHeight3 = height;
                }
            }
        });
    }

    function showWin3() {
        // 若核撥庫房為藥局,排序預設為院內碼,否則為儲位
        if (T3Form.getForm().findField("T3P6").getValue())
            chkDefSort();

        if (win3.hidden) {
            win3.show();
        }
    }
    function hideWin3() {
        if (!win3.hidden) {
            win3.hide();
        }
    }
    function showReportT3() {
        if (!win) {
            
            var winform = Ext.create('Ext.form.Panel', {
                id: 'iframeReport',
                layout: 'fit',
                closable: false,
                html: '<iframe src="' + reportUrl + '?'
                //+ 'docno=' + T1LastRec.data["DOCNO"] + '&'
                    + 'fr=AA0015&'
                    + 'p0=' + T3Form.getForm().findField('T3P0').getValue() + '&'
                    + 'p1=' + T3Form.getForm().findField('T3P1').getValue() + '&'
                    + 'p2=' + T3Form.getForm().findField('T3P2').rawValue + '&'
                    + 'p3=' + T3Form.getForm().findField('T3P3').rawValue + '&'
                    + 'p4=' + T3Form.getForm().findField('T3P4').getValue() + '&'
                    + 'p5=' + VvRadio + '&'
                    + 'p6=' + T3Form.getForm().findField('T3P6').getValue()
                    + '" id="mainContent" name="mainContent" width="100%" height="100%" frameborder="0" style="background-color:#FFFFFF"></iframe>',
                buttons: [{
                    text: '關閉',
                    handler: function () {
                        this.up('window').destroy();
                    }
                }]
            });
            var win = GetPopWin(viewport, winform, '', viewport.width - 20, viewport.height - 20);
        }
        win.show();
    }
    function showReport() {
        if (!win) {
            var winform = Ext.create('Ext.form.Panel', {
                id: 'iframeReport',
                //height: '100%',
                //width: '100%',
                layout: 'fit',
                closable: false,
                html: '<iframe src="' + reportUrl + '?'
                + 'docno=' + T1LastRec.data["DOCNO"]
                + '" id="mainContent" name="mainContent" width="100%" height="100%" frameborder="0" style="background-color:#FFFFFF"></iframe>',
                buttons: [{
                    text: '關閉',
                    handler: function () {
                        this.up('window').destroy();
                    }
                }]
            });
            var win = GetPopWin(viewport, winform, '', viewport.width - 20, viewport.height - 20);
        }
        win.show();
    }

    //#region 2020-07-20: 新增退回功能
    var T4Form = Ext.widget({
        xtype: 'form',
        layout: 'form',
        frame: false,
        title: '',
        fieldDefaults: {
            msgTarget: 'side',
            labelWidth: 90
        },
        autoScroll: true,
        items: [
            {
                xtype: 'displayfield',
                name:'DOCNO',
                fieldLabel: '單據號碼',
                labelAlign: 'right',
            },
            {
                xtype: 'textfield',
                fieldLabel: '退回說明',
                labelAlign: 'right',
                name: 'RETURN_NOTE'
            },
        ]
    });

    var returnWindow = Ext.create('Ext.window.Window', {
        renderTo: Ext.getBody(),
        modal: true,
        id: 'returnWindow',
        items: [T4Form],
        resizable: true,
        draggable: true,
        closable: false,
        title: "退回",
        buttons: [
            {
                text: '確定',
                handler: function () {
                    Ext.Ajax.request({
                        url: '/api/AA0015/Return',
                        method: reqVal_p,
                        params: {
                            docno: T4Form.getForm().findField('DOCNO').getValue(),
                            return_note: T4Form.getForm().findField('RETURN_NOTE').getValue(),
                        },
                        success: function (response) {
                            var data = Ext.decode(response.responseText);
                            if (data.success) {
                                msglabel('退回成功');
                                T1Load();
                                T2Store.removeAll();
                                returnWindow.hide();
                                return;
                            }
                            Ext.MessageBox.alert('錯誤', data.msg);
                        },
                        failure: function (response) {
                            Ext.MessageBox.alert('錯誤', '發生例外錯誤');
                        },
                    });


                    
                }
            },
            {
                text: '取消',
                handler: function () {
                    
                    returnWindow.hide();
                }
            }
        ],
        listeners: {
            show: function (self, eOpts) {
                T4Form.getForm().findField('RETURN_NOTE').setValue('');
                returnWindow.center();
            }
        }
    });
    returnWindow.hide();
    //#endregion

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
        //plugins: [T2RowEditing],
        plugins: [T2CellEditing],
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
                text: "單據號碼",
                dataIndex: 'DOCNO',
                width: 100
            },
            {
                text: "項次",
                dataIndex: 'SEQ',
                width: 60,
                align: 'right',
            },
            {
                text: "院內碼",
                dataIndex: 'MMCODE',
                width: 100,
                renderer: function (val, meta, record) {
                    if (record.data['WEXP_ID'] == 'Y')
                        return '<font color=red>' + val + '</font>';
                    else
                        return val;
                }
            },
            {
                text: "品名",
                dataIndex: 'MMNAME_E',
                width: 200
            },
            {
                text: "單位",
                dataIndex: 'BASE_UNIT',
                width: 60
            },
            {
                text: "安全量",
                dataIndex: 'SAFE_QTY',
                width: 70,
                align: 'right',
                style: 'text-align:left'
            },
            {
                text: "作業量",
                dataIndex: 'OPER_QTY',
                width: 70,
                align: 'right',
                style: 'text-align:left'
            },
            {
                text: "核發庫存量",
                dataIndex: 'S_INV_QTY',
                width: 90,
                align: 'right',
                style: 'text-align:left'
            },
            {
                text: '軍品量',
                dataIndex: 'ARMY_QTY',
                width: 90,
                align: 'right',
                style: 'text-align:left'
            },
            {
                text: '軍民總量',
                dataIndex: 'ARMY_TOTAL_QTY',
                width: 90,
                align: 'right',
                style: 'text-align:left'
            },
            {
                text: "申請量",
                dataIndex: 'APPQTY',
                width: 70,
                align: 'right',
                style: 'text-align:left'
            },
            {
                text: "<span style='color:red'>核撥量</span>",
                dataIndex: 'APVQTY',
                width: 70,
                align: 'right',
                style: 'text-align:left',
                editor: {
                    //xtype: 'textfield',
                    //regexText: '只能輸入數字',
                    //regex: /^[0-9]+$/, // 用正規表示式限制可輸入內容
                    //allowBlank: false,
                    //fieldCls: 'required'
                    //xtype: 'numberfield',
                    fieldCls: 'required',
                    selectOnFocus: true,
                    allowBlank: false,
                    minValue: 0,
                    // Remove spinner buttons, and arrow key and mouse wheel listeners
                    hideTrigger: true,
                    keyNavEnabled: false,
                    mouseWheelEnabled: false,
                    listeners: {
                        specialkey: function (field, e) {
                            if (e.getKey() === e.UP) {
                                arrow = 'UP';
                                var editPlugin = this.up().editingPlugin;
                                editPlugin.completeEdit();
                                var sm = T2Grid.getSelectionModel();
                                sm.deselectAll();
                                sm.select(editPlugin.context.rowIdx - 1);
                                editPlugin.startEdit(editPlugin.context.rowIdx - 1, 13);
                            }

                            if (e.getKey() === e.DOWN || e.getKey() === e.ENTER) {
                                arrow = 'DOWN';
                                var editPlugin = this.up().editingPlugin;
                                editPlugin.completeEdit();
                                var sm = T2Grid.getSelectionModel();
                                sm.deselectAll();
                                sm.select(editPlugin.context.rowIdx + 1);
                                editPlugin.startEdit(editPlugin.context.rowIdx + 1, 13);
                            }
                        }
                    }
                }
            },
            {
                text: "點收量",
                dataIndex: 'ACKQTY',
                width: 70,
                align: 'right',
                style: 'text-align:left'
            },
            {
                text: "儲位",
                dataIndex: 'STORE_LOC',
                width: 100
            },
            {
                text: '過帳狀態',
                dataIndex: 'POSTID',
                width: 120
            },
            {
                text: '在途量',
                dataIndex: 'ONWAY_QTY',
                width: 80,
                align: 'right',
                style: 'text-align:left'
            },
            {
                text: '核撥時間',
                dataIndex: 'APVTIME',
                width: 120
            },
            {
                text: '核撥人員',
                dataIndex: 'APVID',
                width: 120
            },
            {
                text: '點收時間',
                dataIndex: 'ACKTIME',
                width: 120
            },
            {
                text: '點收人員',
                dataIndex: 'ACKID',
                width: 120
            },
            {
                text: '強迫點收數量',
                dataIndex: 'ACKSYSQTY',
                width: 120,
                align: 'right',
                style: 'text-align:left'
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

                }
            },
            selectionchange: function (model, records) {
                
                T2LastRec = records[0];
                if (T2LastRec != null) {
                    if (T2LastRec.data["POSTID"] == '待核撥') {
                        Ext.getCmp('detailSubmit').setDisabled(false);
                    }
                    else {

                        Ext.getCmp('detailSubmit').setDisabled(true);
                    }
                    if (T2LastRec.data["POSTIDC"] == 'C') {
                        Ext.getCmp('detailCancel').setDisabled(false);
                    }
                    else {

                        Ext.getCmp('detailCancel').setDisabled(true);
                    }
                }
            }
        }
    });

    function chkWexpid(parSelection) {
        let name = '';
        let docno = '';
        $.map(parSelection, function (item, key) {
            name += '「' + item.get('DOCNO') + '」<br>';
            docno += item.get('DOCNO') + ',';
        })

        Ext.Ajax.request({
            url: chkWexpidGet,
            params: {
                docno: docno
            },
            method: reqVal_p,
            success: function (response) {
                var data = Ext.decode(response.responseText);
                if (data.success) {
                    if (data.msg == 'Y') {                      
                        //Ext.MessageBox.confirm('核撥', '確認是否核撥單據號碼「' + T1LastRec.data["DOCNO"] + '」？', function (btn, text) {
                        Ext.MessageBox.confirm('核撥', '確認是否核撥單據號碼<br>' + name, function (btn, text) {
                            if (btn === 'yes') {
                                //var data = { item: [] };
                                //data.item.push(T1LastRec.data["DOCNO"]);
                                Ext.Ajax.request({
                                    //url: '/api/AA0015/UpdateMeDocmStatus?docno=' + T1LastRec.data["DOCNO"],
                                    //url: '/api/AA0015/UpdateMeDocmStatus',
                                    url: '/api/AA0015/UpdateStatusBySP',
                                    method: reqVal_p,
                                    params: {
                                        DOCNO: docno
                                    },
                                    //async: true,
                                    success: function (response) {
                                        var data = Ext.decode(response.responseText);
                                        if (data.success) {
                                            msglabel('核撥成功');
                                            T2Store.removeAll();
                                            T1Grid.getSelectionModel().deselectAll();
                                            T1Load();
                                            Ext.getCmp('btnSubmit').setDisabled(true);
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
                        Ext.Msg.alert('訊息', data.msg + '效期管制資料尚未維護完成。');
                    }
                }
            },
            failure: function (response, options) {

            }
        });
    }

    function chkWexpidDetail(parDocno, parSeq) {
        Ext.Ajax.request({
            url: chkWexpidDetailGet,
            params: {
                docno: parDocno,
                seq: parSeq
            },
            method: reqVal_p,
            success: function (response) {
                var data = Ext.decode(response.responseText);
                if (data.success) {
                    if (data.msg == 'Y') {
                        //Ext.MessageBox.confirm('核撥', '確認是否核撥單據號碼「' + T1LastRec.data["DOCNO"] + '」？', function (btn, text) {
                        Ext.MessageBox.confirm('完成揀料', '確認是否送出?', function (btn, text) {
                            if (btn === 'yes') {
                                //var data = { item: [] };
                                //data.item.push(T1LastRec.data["DOCNO"]);
                                Ext.Ajax.request({
                                    //url: '/api/AA0015/UpdateMeDocmStatus?docno=' + T1LastRec.data["DOCNO"],
                                    //url: '/api/AA0015/UpdateMeDocmStatus',
                                    url: '/api/AA0015/UpdatePostidBySP',
                                    method: reqVal_p,
                                    params: {
                                        DOCNO: parDocno,
                                        SEQ: parSeq
                                    },
                                    //async: true,
                                    success: function (response) {
                                        var data = Ext.decode(response.responseText);
                                        if (data.success) {
                                            //T2Store.removeAll();
                                            //T1Grid.getSelectionModel().deselectAll();
                                            T1Load();
                                            T1LastRec = null;
                                            T2Load();
                                            Ext.getCmp('detailSubmit').setDisabled(true);
                                            msglabel('完成揀料成功');
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
                        Ext.Msg.alert('訊息', '項次' + data.msg + '效期管制資料尚未維護完成。');
                    }
                }
            },
            failure: function (response, options) {

            }
        });
    }

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
                title: '查詢',
                border: false,
                layout: {
                    type: 'fit',
                    padding: 5,
                    align: 'stretch'
                },
                items: [T1Query]
            }
        ]
    });
    //Ext.getCmp('eastform').collapse();
    Ext.getCmp('eastform').expand();
    //T1Load();

    //T1Query.getForm().reset();
    //T1Query.getForm().findField('D0').setValue(getTodayDate());
    //T1Query.getForm().findField('D1').setValue(getTodayDate());
    //Ext.getCmp('export').setDisabled(true);
    //Ext.getCmp('print').setDisabled(true);
    //Ext.getCmp('print_non').setDisabled(true);
    Ext.getCmp('detailSubmit').setDisabled(true);
    Ext.getCmp('detailCancel').setDisabled(true);
    T1Query.getForm().findField('P8').setValue(arrP8);
});