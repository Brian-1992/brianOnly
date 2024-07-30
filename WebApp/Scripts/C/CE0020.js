Ext.Loader.setConfig({
    enabled: true,
    paths: {
        'WEBAPP': '/Scripts/app'
    }
});

Ext.require(['WEBAPP.utils.Common']);

Ext.onReady(function () {
    var T1Set = '';
    var WhnoComboGet = '../../../api/CE0020/GetWhnoCombo';

    var userId = session['UserId'];
    var userName = session['UserName'];
    var userInid = session['Inid'];
    var userInidName = session['InidName'];
    var windowHeight = $(window).height();
    //var userId, userName, userInid, userInidName;
    var T1LastRec = null;
    var T22LastRec = null;
    var loadT22 = false;
    var sCHK_STATUS = '';
    var sCHK_NO = '';
    var filter_e_orderdcflag = 'N';

    var todayDateString = '';
    function getTodayDate() {
        Ext.Ajax.request({
            url: '/api/CE0002/CurrentDate',
            method: reqVal_g,
            success: function (response) {
                var data = Ext.decode(response.responseText);
                if (data.success) {
                    todayDateString = data.msg;
                    setComboData();
                }
            },
            failure: function (response, options) {

            }
        });
    }
    getTodayDate();
    function isEditable(chk_ym) {
        
        if (chk_ym.substring(0, 5) != todayDateString) {
            return false;
        } else {
            return true;
        }
    }

    var viewModel = Ext.create('WEBAPP.store.CE0020VM');

    // 庫房清單
    var whnoQueryStore = Ext.create('Ext.data.Store', {
        fields: ['VALUE', 'TEXT']
    });
    function setComboData() {
        Ext.Ajax.request({
            url: WhnoComboGet,
            method: reqVal_g,
            success: function (response) {
                var data = Ext.decode(response.responseText);
                if (data.success) {
                    whnoQueryStore.add({ WH_NO: '', WH_KIND: '', WH_NAME: '', WH_GRADE: '' });
                    var wh_nos = data.etts;
                    if (wh_nos.length > 0) {
                        for (var i = 0; i < wh_nos.length; i++) {
                            whnoQueryStore.add(wh_nos[i]);
                        }
                        T1Query.getForm().findField('P0').setValue(wh_nos[0].WH_NO);
                        T1Load(true);
                    }
                }
            },
            failure: function (response, options) {

            }
        });
    }

    var T1Store = viewModel.getStore('MasterAll');
    function T1Load(clearMsg) {

        if (clearMsg) {
            msglabel('');
        }

        //T1LastRec = null;

        var p0 = T1Query.getForm().findField('P0').getValue();
        if (p0 == null) {
            p0 = "";
        }

        T1Store.getProxy().setExtraParam("p0", p0);
        T1Store.getProxy().setExtraParam("p1", T1Query.getForm().findField('P1').rawValue);
        T1Store.getProxy().setExtraParam("p2", userId);
        T1Tool.moveFirst();
    }

    function getColumnIndex(columns, dataIndex) {
        var index = -1;
        for (var i = 0; i < columns.length; i++) {
            if (columns[i].dataIndex == dataIndex) {
                index = i;
            }
        }

        return index;
    }

    // 查詢欄位
    var mLabelWidth = 70;
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
        items: [{
            xtype: 'container',
            layout: 'hbox',
            items: [
                {
                    xtype: 'panel',
                    id: 'PanelP1',
                    border: false,
                    layout: 'hbox',
                    items: [
                        {
                            xtype: 'combo',
                            store: whnoQueryStore,
                            name: 'P0',
                            id: 'P0',
                            fieldLabel: '庫房代碼',
                            displayField: 'WH_NAME',
                            valueField: 'WH_NO',
                            queryMode: 'local',
                            anyMatch: true,
                            allowBlank: true,
                            typeAhead: true,
                            forceSelection: true,
                            triggerAction: 'all',
                            multiSelect: false,
                            tpl: '<tpl for="."><div class="x-boundlist-item" style="height:auto;">{WH_NAME}&nbsp;</div></tpl>',
                        },
                        {
                            xtype: 'monthfield',
                            fieldLabel: '盤點年月',
                            name: 'P1',
                            id: 'P1',
                            // enforceMaxLength: true,
                            //maxLength: 5,
                            //minLength: 5,
                            //regexText: '請填入民國年月',
                            //regex: /\d{5,5}/,
                            labelWidth: mLabelWidth,
                            width: 180,
                            padding: '0 4 0 4',
                            //format: 'Xm',
                            value: new Date()
                        },
                        {
                            xtype: 'displayfield',
                            fieldLabel: '負責人',
                            name: 'P2',
                            id: 'P2',
                            enforceMaxLength: true,
                            maxLength: 21,
                            labelWidth: 60,
                            width: 210,
                            padding: '0 4 0 4',
                            value: userId + ' ' + userName
                        },
                        {
                            xtype: 'button',
                            text: '查詢',
                            handler: function () {
                                msglabel('訊息區:');
                                var f = T1Query.getForm();
                                if (!f.findField('P0').getValue() &&
                                    !f.findField('P1').getValue()) {
                                    Ext.Msg.alert('提醒', '<span style=\'color:red\'>庫房代碼</span>與<span style=\'color:red\'>盤點年月</span>至少需填寫一項');
                                    return;
                                }

                                T1Load(true);

                                //Ext.getCmp('eastform').collapse();
                            }
                        },
                        {
                            xtype: 'button',
                            text: '清除',
                            handler: function () {
                                var f = this.up('form').getForm();
                                f.reset();
                                f.findField('P0').focus(); // 進入畫面時輸入游標預設在P0
                            }
                        }
                    ]
                }
            ]
        }]
    });

    var T1Tool = Ext.create('Ext.PagingToolbar', {
        store: T1Store,
        border: false,
        displayInfo: true,
        plain: true,
    });
   
    var T1Grid = Ext.create('Ext.grid.Panel', {
        store: T1Store,
        plain: true,
        loadingText: '處理中...',
        loadMask: true,
        cls: 'T1',
        dockedItems: [
            {
                dock: 'top',
                xtype: 'toolbar',
                layout: 'fit',
                items: [T1Query]
            },
            {
                dock: 'top',
                xtype: 'toolbar',
                items: [T1Tool]
            }
        ],
        columns: [
            {
                xtype: 'rownumberer'
            },
            {
                text: "庫房代碼",
                dataIndex: 'WH_NAME',
                width: 120
            },
            {
                text: "盤點年月日",
                dataIndex: 'CHK_YM',
                width: 80
            },
            {
                text: "庫房級別",
                dataIndex: 'CHK_WH_GRADE',
                width: 70
            },
            {
                text: "庫房分類",
                dataIndex: 'CHK_WH_KIND',
                width: 80
            },
            {
                text: "物料分類",
                dataIndex: 'CHK_CLASS',
                width: 80
            },
            {
                text: "盤點期",
                dataIndex: 'CHK_PERIOD',
                width: 70,
                xtype: 'templatecolumn',
                tpl: '{CHK_PERIOD}'
            },
            {
                text: "盤點類別",
                dataIndex: 'CHK_TYPE',
                width: 120,
                xtype: 'templatecolumn',
                tpl: '{CHK_TYPE}'
            },
            {
                text: "盤點量/總量",
                dataIndex: 'QTY',
                width: 120,
                xtype: 'templatecolumn',
                tpl: '{QTY}'
            },
            {
                text: "盤點單號",
                dataIndex: 'CHK_NO',
                width: 120,
                renderer: function (val, meta, record) {
                    var CHK_NO = record.data.CHK_NO;
                    return '<a href=javascript:void(0)>' + CHK_NO + '</a>';
                },
            },
            {
                text: "負責人員",
                dataIndex: 'CHK_KEEPER',
                width: 120
            },
            {
                text: "狀態",
                dataIndex: 'CHK_STATUS_NAME',
                width: 80
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
                T1Rec = records.length;

                if (records.length > 0) {
                    T1LastRec = Ext.clone(records[0]);
                }

            },
            cellclick: function (self, td, cellIndex, record, tr, rowIndex, e, eOpts) {
                var columns = T1Grid.getColumns();
                var index = getColumnIndex(columns, 'CHK_NO');
                msglabel('');
                if (index != cellIndex) {
                    return;
                }

                T1LastRec = Ext.clone(record);

                sCHK_NO = record.data.CHK_NO;
                sCHK_STATUS = record.data.CHK_STATUS.substring(0, 1);

                var f = filterForm.getForm();
                f.findField('ORI_GAP_T').setValue(0);
                f.findField('ORI_GAP_PRICE').setValue(0);
                f.findField('ORI_M_CONTPRICE').setValue(0);
                f.findField('ORI_GAP_P').setValue(0);
                f.findField('ORI_E_ORDERDCFLAG').setValue('N');
                f.findField('ORI_DRUG_TYPE').setValue('');
                f.findField('GAP_T').setValue(0);
                f.findField('GAP_PRICE').setValue(0);
                f.findField('M_CONTPRICE').setValue(0);
                f.findField('GAP_P').setValue(0);
                f.findField('E_ORDERDCFLAG').setValue({ E_ORDERDCFLAG: 'N' });
                f.findField('DRUG_TYPE').setValue('');

                if (Number(T1LastRec.data.CHK_STATUS) == 2) {
                    Ext.getCmp('btnModifiedSheet').enable();
                    Ext.getCmp('btnSaveEdit').enable();

                } else {
                    Ext.getCmp('btnModifiedSheet').disable();
                    Ext.getCmp('btnSaveEdit').disable();
                }

                if (Number(T1LastRec.data.CHK_STATUS) == 2 || Number(T1LastRec.data.CHK_STATUS) == 1) {
                    Ext.getCmp('btnSaveMemo').enable();
                } else {
                    Ext.getCmp('btnSaveMemo').disable();
                }
               

                T21Load(false);

                var title = record.data.CHK_NO + ' ' + record.data.WH_NAME + ' ' + record.data.CHK_YM + ' ' + record.data.CHK_PERIOD + ' ' + record.data.CHK_WH_KIND
                detailWindow.setTitle('盤點明細管理 ' + title);

                detailWindow.center().show();
            },
        }
    });

    // #region 盤點項目編輯
    var T21Store = viewModel.getStore('DetailAll');
    T21Store.on('beforeload', function (store, options) {

        T21Store.getProxy().setExtraParam("gap_t", filterForm.getForm().findField('GAP_T').getValue());
        T21Store.getProxy().setExtraParam("gap_price", filterForm.getForm().findField('GAP_PRICE').getValue());
        T21Store.getProxy().setExtraParam("m_contprice", filterForm.getForm().findField('M_CONTPRICE').getValue());
        T21Store.getProxy().setExtraParam("gap_p", filterForm.getForm().findField('GAP_P').getValue());
        T21Store.getProxy().setExtraParam("e_orderdcflag", filterForm.getForm().findField('E_ORDERDCFLAG').getValue());
        T21Store.getProxy().setExtraParam("drug_type", filterForm.getForm().findField('DRUG_TYPE').getValue());
        T21Store.getProxy().setExtraParam("seq1", filterForm.getForm().findField('SEQ1').getValue());
        T21Store.getProxy().setExtraParam("seq2", filterForm.getForm().findField('SEQ2').getValue());
        T21Store.getProxy().setExtraParam("mmcode1", filterForm.getForm().findField('MMCODE1').getValue());
        T21Store.getProxy().setExtraParam("mmcode2", filterForm.getForm().findField('MMCODE2').getValue());
        T21Store.getProxy().setExtraParam("mmname", filterForm.getForm().findField('MMNAME').getValue());

    });
    function T21Load(isLoadT22, isMoveFirst) {
        loadT22 = isLoadT22;
        T21Store.removeAll();
        T22Store.removeAll();

        //T21Query.getForm().findField('T1P0').setValue(T1Query.getForm().findField('P0').getValue());
        T21Store.getProxy().setExtraParam("chk_no", sCHK_NO);
        T21Store.getProxy().setExtraParam("chk_status", sCHK_STATUS);

        if (isEditable(T1LastRec.data.CHK_YM) == false) {
            Ext.getCmp('btnModifiedSheet').disable();
            Ext.getCmp('btnSaveEdit').disable();
            Ext.getCmp('btnSaveMemo').disable();

            if (isMoveFirst) {
                T21Tool.moveFirst();
            } else {
                T21Store.load({
                    params: {
                        start: 0
                    }
                });
            }
            return;
        }

        if (sCHK_STATUS == "2") {
            Ext.Ajax.request({
                url: '/api/CE0020/IsStoreqtyEmpty',
                method: reqVal_p,
                params: { chk_no: sCHK_NO },
                success: function (response) {
                    var data = Ext.decode(response.responseText);
                    if (data.msg == 'Y') {
                        Ext.getCmp('btnModifiedSheet').disable();
                    } else {
                        Ext.getCmp('btnModifiedSheet').enable();

                    }
                },
                failure: function (response, options) {

                }
            });
        }
        else {
            Ext.getCmp('btnModifiedSheet').disable();
        }

        Ext.getCmp('btnSaveEdit').disable();

        if (sCHK_STATUS == "1" || sCHK_STATUS == "2") {
            Ext.getCmp('btnSaveMemo').enable();
        } else {
            Ext.getCmp('btnSaveMemo').disable();
        }

        if (isMoveFirst) {
            T21Tool.moveFirst();
        } else {
            T21Store.load({
                params: {
                    start: 0
                }
            });
        }
    }
    T21Store.on('load', function (store, options) {
        if (loadT22) {
            //T22Load(true);
        }
    });
    var T22Store = viewModel.getStore('UserDetails');
    function T22Load(isMoveFirst) {
        T22Store.getProxy().setExtraParam("mmcode", T21LastRec.data.MMCODE);
        T22Store.getProxy().setExtraParam("chk_no", T21LastRec.data.CHK_NO);
        
        if (isEditable(T1LastRec.data.CHK_YM) == false) {            
            Ext.getCmp('btnSaveEdit').disable();

            if (isMoveFirst) {
                T22Tool.moveFirst();
            } else {
                T22Store.load({
                    params: {
                        start: 0
                    }
                });
            }
            return;
        } 

        if (sCHK_STATUS == "2") {
            Ext.getCmp('btnSaveEdit').enable();
        }
        else {
            Ext.getCmp('btnSaveEdit').disable();
        }

        if (isMoveFirst) {
            T22Tool.moveFirst();
        } else {
            T22Store.load({
                params: {
                    start: 0
                }
            });
        }
    }

    var T21Query = Ext.widget({
        xtype: 'form',
        layout: 'form',
        border: false,
        autoScroll: true, // 若為true,容器內容超出容器大小時出現捲動條;若為false超出容器的部分會被裁切掉
        items: [{
            xtype: 'container',
            layout: 'vbox',
            items: [
                {
                    xtype: 'panel',
                    id: 'PanelP21',
                    border: false,
                    layout: 'hbox',
                    items: [
                        {
                            xtype: 'displayfield',
                            name: 'T21P0',
                            id: 'T21P0',
                            labelWidth: 60,
                            width: 180,
                            fieldLabel: '庫房代碼',
                            padding: '0 4 0 4',
                            labelAlign: 'right',
                        },
                        {
                            xtype: 'displayfield',
                            fieldLabel: '盤點年月',
                            name: 'T21P1',
                            id: 'T21P1',
                            labelWidth: 60,
                            width: 130,
                            padding: '0 4 0 4',
                            labelAlign: 'right',
                        },
                        {
                            xtype: 'displayfield',
                            fieldLabel: '盤點期',
                            name: 'T21P2',
                            id: 'T21P2',
                            labelWidth: 60,
                            width: 130,
                            padding: '0 4 0 4',
                            labelAlign: 'right',
                        },
                        {
                            xtype: 'displayfield',
                            fieldLabel: '盤點類別',
                            name: 'T21P3',
                            id: 'T21P3',
                            labelWidth: 60,
                            width: 150,
                            padding: '0 4 0 4',
                            labelAlign: 'right',
                        },
                        {
                            xtype: 'displayfield',
                            fieldLabel: '狀態',
                            name: 'T21P4',
                            id: 'T21P4',
                            labelWidth: 60,
                            width: 130,
                            padding: '0 4 0 4',
                            labelAlign: 'right',
                        }
                    ]
                },
                {
                    xtype: 'panel',
                    id: 'PanelP22',
                    border: false,
                    layout: 'hbox',
                    items: [
                        {
                            xtype: 'displayfield',
                            fieldLabel: '盤點單號',
                            name: 'T21P6',
                            id: 'T21P6',
                            labelWidth: 60,
                            width: 180,
                            padding: '0 4 0 4',
                            labelAlign: 'right',
                        },
                        {
                            xtype: 'displayfield',
                            name: 'T21P5',
                            id: 'T21P5',
                            labelWidth: 60,
                            width: 130,
                            padding: '0 4 0 4',
                            fieldLabel: '庫房分類',
                            labelAlign: 'right',
                        },
                        {
                            xtype: 'displayfield',
                            fieldLabel: '負責人',
                            name: 'T21P7',
                            id: 'T21P7',
                            labelWidth: 60,
                            width: 150,
                            padding: '0 4 0 4',
                            labelAlign: 'right',
                        }
                    ]
                }
            ]
        }]
    });
    var T21Tool = Ext.create('Ext.PagingToolbar', {
        store: T21Store,
        border: false,
        plain: true,
        displayInfo: true,
        buttons: [
            {
                text: '篩選條件',
                id: 'btnFilter',
                name: 'btnFilter',
                handler: function () {
                    filterWindow.show();
                }
            },
            {
                text: '完成盤點調整',
                id: 'btnModifiedSheet',
                name: 'btnModifiedSheet',
                handler: function () {

                    Ext.MessageBox.confirm('完成盤點調整', '確定完成盤點調整?', function (btn, text) {
                        if (btn === 'yes') {
                            ModifiedSheet(sCHK_NO);
                        }
                    }
                    );
                }
            },
            //{
            //    text: '產生電腦量',
            //    id: 'btnRandom',
            //    name: 'btnRandom',
            //    handler: function () {
                    
            //        Ext.Ajax.request({
            //            url: '/api/CE0020/Random',
            //            method: reqVal_p,
            //            params: { chk_no: sCHK_NO  },
            //            success: function (response) {
            //                var data = Ext.decode(response.responseText);
            //                if (data.success) {
            //                    T21Load(false);
            //                }
            //            },
            //            failure: function (response, options) {

            //            }
            //        });
            //    }
            //},
            {
                text: '儲存備註',
                id: 'btnSaveMemo',
                name: 'btnSaveMemo',
                handler: function () {

                    var tempData = T21Grid.getStore().data.items;
                    var list = [];
                    for (var i = 0; i < tempData.length; i++) {
                        if (tempData[i].dirty) {
                            list.push(tempData[i].data);
                        }
                    }

                    if (list.length == 0) {
                        Ext.Msg.alert('提醒', '無修改的資料');
                        return;
                    }

                   Ext.Ajax.request({
                        url: '/api/CE0020/UpdateMemo',
                        method: reqVal_p,
                        params: { item_string: Ext.util.JSON.encode(list)  },
                        success: function (response) {
                            var data = Ext.decode(response.responseText);
                            if (data.success) {
                                msglabel('備註修改成功');
                                T21Load(false);
                            }
                        },
                        failure: function (response, options) {

                        }
                    });
                    
                }
            },
        ]
    });
    var T22Tool = Ext.create('Ext.PagingToolbar', {
        store: T22Store,
        border: false,
        plain: true,
        displayInfo: true,
        buttons: [

            {
                text: '儲存修改',
                id: 'btnSaveEdit',
                name: 'btnSaveEdit',
                handler: function () {
                    Ext.MessageBox.confirm('儲存修改', '確定儲存修改?', function (btn, text) {
                        if (btn === 'yes') {
                            SaveEdit();
                        }
                    }
                    );
                }
            },
        ]
    });
    var T21Grid = Ext.create('Ext.grid.Panel', {
        store: T21Store,
        plain: true,
        loadingText: '處理中...',
        loadMask: true,
        cls: 'T1',
        height: windowHeight / 2 - 65,
        dockedItems: [
            {
                dock: 'top',
                xtype: 'toolbar',
                items: [T21Tool]
            }
        ],
        columns: [
            {
                text: "",
                dataIndex: 'SEQ',
                width: 40,
                align: 'right',
                style: 'text-align:left',
            },
            {
                text: "院內碼",
                dataIndex: 'MMCODE',
                width: 120
            },
            {
                text: "中文名稱",
                dataIndex: 'MMNAME_C',
                width: 150
            },
            {
                text: "英文名稱",
                dataIndex: 'MMNAME_E',
                width: 150
            },
            {
                text: "計量單位",
                dataIndex: 'BASE_UNIT',
                width: 65,
            },
            {
                text: "電腦量",
                dataIndex: 'STORE_QTY',
                //width: 60,
                align: 'right',
                style: 'text-align:left',
                width: 65,
            },
            {
                text: "盤點總量",
                dataIndex: 'CHK_QTY',
                width: 70,
                align: 'right',
                style: 'text-align:left'
            },
            {
                text: "誤差量",
                dataIndex: 'QTY_DIFF',
                width: 65,
                align: 'right',
                style: 'text-align:left'
            },
            {
                text: "價格",
                dataIndex: 'M_CONTPRICE',
                width: 65,
                align: 'right',
                style: 'text-align:left'
            },
            {
                text: "盤點時間",
                dataIndex: 'CHK_TIME',
                width: 65,
                style: 'text-align:left'
            },
            {
                text: '<span style="color:red">備註</span>',
                dataIndex: 'MEMO',
                width: 120,
                editor: {
                    xtype: 'textfield',
                    maxLength: 50
                },
            },
            {
                header: '',
                flex:1
            }
        ],
        listeners: {
            selectionchange: function (model, records) {
                T21Rec = records.length;
                T21LastRec = records[0];
                if (T21Rec) {
                    T22Load(true);
                }
            },
            beforeedit: function (editor, e) {
                if (isEditable(T1LastRec.data.CHK_YM) == false) {
                    return false;
                }
                if (sCHK_STATUS != '2' && sCHK_STATUS != '1') {
                    return false;
                }
                return true;
            }
        },
        plugins: [
            Ext.create('Ext.grid.plugin.CellEditing', {
                clicksToEdit: 1
            })
        ],
        viewConfig: {
            listeners: {
                itemkeydown: function (grid, rec, item, idx, e) {
                    if (e.keyCode == 38) { //上
                        e.preventDefault();
                        var editPlugin = this.up().editingPlugin;
                        editPlugin.startEdit(editPlugin.context.rowIdx - 1, editPlugin.context.colIdx);
                    } else if (e.keyCode == 40) { //下
                        e.preventDefault();
                        var editPlugin = this.up().editingPlugin;
                        editPlugin.startEdit(editPlugin.context.rowIdx + 1, editPlugin.context.colIdx);
                    }

                    //if (e.keyCode == 13) { //enter
                    //    T3Load();
                    //}

                    //alert('The press key is' + e.getKey());
                }
            }
        },
    });

    var T22CellEditing = Ext.create('Ext.grid.plugin.CellEditing', {
        clicksToEdit: 1,
        autoCancel: true,
        listeners: {
            beforeedit: function (editor, context, eOpts) {
                if (sCHK_STATUS == '2') {
                    return true;
                }
                else {
                    return false
                }
            }
        }
    });

    var T22Grid = Ext.create('Ext.grid.Panel', {
        store: T22Store,
        plain: true,
        loadingText: '處理中...',
        loadMask: true,
        cls: 'T1',
        plugins: [T22CellEditing],
        height: windowHeight / 2 - 62,
        dockedItems: [
            {
                dock: 'top',
                xtype: 'toolbar',
                items: [T22Tool]
            }
        ],
        columns: [
            {
                xtype: 'rownumberer'
            },
            {
                text: "盤點人員",
                dataIndex: 'CHK_UID_NAME',
                width: 180,
                style: 'text-align:left'
            },
            {
                text: "盤點總量",
                dataIndex: 'CHK_QTY',
                width: 100,
                align: 'right',
                style: 'text-align:left; color:red',
                editor: {
                    xtype: 'textfield',
                    regexText: '只能輸入數字',
                    regex: /^[0-9]+$/, // 用正規表示式限制可輸入內容
                    listeners: {

                    }
                },
            }
        ],
        viewConfig: {
            listeners: {
                itemkeydown: function (grid, rec, item, idx, e) {
                    if (e.keyCode == 38) { //上
                        e.preventDefault();
                        var editPlugin = this.up().editingPlugin;
                        editPlugin.startEdit(editPlugin.context.rowIdx - 1, editPlugin.context.colIdx);
                    } else if (e.keyCode == 40) { //下
                        e.preventDefault();
                        var editPlugin = this.up().editingPlugin;
                        editPlugin.startEdit(editPlugin.context.rowIdx + 1, editPlugin.context.colIdx);
                    }
                }
            }
        }
    });

    var ModifiedSheet = function (CHK_NO) {
        var myMask = new Ext.LoadMask(Ext.getCmp('detailWindow'), { msg: '處理中...' });
        myMask.show();
        Ext.Ajax.request({
            url: '/api/CE0020/ModifiedSheet',
            method: reqVal_p,
            params: {
                CHK_NO: CHK_NO
            },
            success: function (response) {
                //windowNewOpen = true;
                //T1LastRec.data.CHK_STATUS = '1';

                var data = Ext.decode(response.responseText);
                if (data.success) {
                    myMask.hide();
                    sCHK_STATUS = '3';
                    T21Load(true);
                    //T22Load();
                    T1Load(false);

                    msglabel("調整盤點單成功");

                    Ext.getCmp('btnModifiedSheet').disable();
                    Ext.getCmp('btnSaveEdit').disable();
                    Ext.getCmp('btnSaveMemo').disable();
                    //Ext.getCmp('btnRandom').disable();
                } else {
                    myMask.hide();
                    Ext.Msg.alert('失敗', '發生例外錯誤');
                }

            },
            failure: function (response, options) {
                Ext.Msg.alert('失敗', '發生例外錯誤');
            }
        });
    }

    var SaveEdit = function () {
        
        var store = T22Grid.getStore();

        var list = [];
        //for()
        for (var i = 0; i < store.data.items.length; i++) {
            var temp = {
                CHK_NO: store.data.items[i].data.CHK_NO,
                CHK_UID: store.data.items[i].data.CHK_UID,
                MMCODE: store.data.items[i].data.MMCODE,
                CHK_QTY: store.data.items[i].data.CHK_QTY
            };
            list.push(temp);
        }

        Ext.Ajax.request({
            url: '/api/CE0020/SaveEdit',
            method: reqVal_p,
            params: {
                item_string: Ext.util.JSON.encode(list)
            },
            success: function (response) {

                T21Store.removeAll();
                T22Store.removeAll();

                var data = Ext.decode(response.responseText);
                if (data.success == false) {
                    Ext.Msg.alert('失敗', data.msg);
                    return;
                }

                T21Load(true);
                //T22Load();

                msglabel("修改成功");
            },
            failure: function (response, options) {
                Ext.Msg.alert('失敗', '發生例外錯誤');
            }
        });
    }

    var detailWindow = Ext.create('Ext.window.Window', {
        renderTo: Ext.getBody(),
        id:'detailWindow',
        modal: true,
        items: [
            {
                xtype: 'container',
                layout: 'fit',
                items: [
                    //T21Query,
                    {
                        xtype: 'panel',
                        itemId: 't21Grid',
                        region: 'center',
                        layout: 'fit',
                        collapsible: false,
                        title: '完成盤點調整清單',
                        border: false,
                        height: '49%',
                        minHeight: windowHeight / 2 - 90,
                        items: [T21Grid]
                    },
                    {
                        xtype: 'splitter',
                        collapseTarget: 'dev'
                    },
                    //T22Query,
                    {
                        xtype: 'form',
                        autoScroll: true,
                        itemId: 't22Grid',
                        region: 'south',
                        layout: 'fit',
                        //collapsible: true,
                        title: '盤點調整清單',
                        //titleCollapse: true,
                        border: false,
                        height: '50%',
                        minHeight: 30,
                        split: true,
                        items: [T22Grid]
                    }
                ],
            }
        ],
        width: "900px",
        height: windowHeight,
        resizable: false,
        draggable: true,
        closable: false,
        //y: 0,
        title: "盤點明細管理",
        buttons: [{
            text: '關閉',
            handler: function () {
                detailWindow.hide();
            }
        }]
    });
    detailWindow.hide();

    //#region filterWindow
    var drugtypeStore = Ext.create('Ext.data.Store', {
        fields: ['VALUE', 'TEXT'],
        data: [
            { "VALUE": "", "TEXT": "全部" },
            { "VALUE": "1", "TEXT": "管制藥" },
            { "VALUE": "2", "TEXT": "高價藥" },
            { "VALUE": "3", "TEXT": "研究用藥" },
            { "VALUE": "4", "TEXT": "罕見病藥" },
            { "VALUE": "5", "TEXT": "公費藥" }
        ]
    });
    var filterMMCode1 = Ext.create('WEBAPP.form.MMCodeCombo', {
        id: 'filterMmcode1',
        name: 'MMCODE1',
        fieldLabel: '院內碼',
        labelAlign: 'right',
        labelSeparator: '',
        width: 200,
        labelWidth: 100,
        limit: 10, //限制一次最多顯示10筆
        queryUrl: '/api/CE0002/GetMMCODECombo',//指定查詢的Controller路徑
        extraFields: ['MMNAME_C', 'MMNAME_E'], //查詢完會回傳的欄位
        getDefaultParams: function () { //查詢時Controller固定會收到的參數
            return {
                p1: T1LastRec.data.CHK_NO  //P0:預設是動態MMCODE
            };
        },
        listeners: {
            select: function (c, r, i, e) {
                //選取下拉項目時，顯示回傳值
            }
        }
    });

    var filterMMCode2 = Ext.create('WEBAPP.form.MMCodeCombo', {
        id: 'filterMmcode2',
        name: 'MMCODE2',
        fieldLabel: '至',
        labelAlign: 'right',
        labelWidth: 7,
        width: 107,
        limit: 10, //限制一次最多顯示10筆
        queryUrl: '/api/CE0002/GetMMCODECombo',//指定查詢的Controller路徑
        extraFields: ['MMNAME_C', 'MMNAME_E'], //查詢完會回傳的欄位
        getDefaultParams: function () { //查詢時Controller固定會收到的參數
            return {
                p1: T1LastRec.data.CHK_NO  //P0:預設是動態MMCODE
            };
        },
        listeners: {
            select: function (c, r, i, e) {
                //選取下拉項目時，顯示回傳值
            }
        }
    });

    var filterForm = Ext.widget({
        xtype: 'form',
        layout: 'form',
        border: false,
        autoScroll: true, // 若為true,容器內容超出容器大小時出現捲動條;若為false超出容器的部分會被裁切掉
        fieldDefaults: {
            labelAlign: 'right',
        },
        items: [{
            xtype: 'container',
            layout: 'vbox',
            items: [
                {
                    xtype: 'panel',
                    id: 'PanelP24',
                    border: false,
                    layout: 'hbox',
                    items: [
                        {
                            name: 'ORI_SEQ1',  
                            xtype: 'hidden',
                            value: 0
                        },
                        {
                            name: 'ORI_SEQ2', 
                            xtype: 'hidden',
                            value: 0
                        },
                        {
                            xtype: 'numberfield',
                            fieldLabel: '項次',
                            name: 'SEQ1',
                            enforceMaxLength: false,
                            maxLength: 14,
                            width:150,
                            hideTrigger: true,
                            minValue: 1,
                            labelSeparator: '',
                        },
                        {
                            xtype: 'numberfield',
                            fieldLabel: '至',
                            name: 'SEQ2',
                            enforceMaxLength: false,
                            maxLength: 14,
                            labelWidth: 8,
                            width: 58,
                            hideTrigger: true,
                            minValue: 1,
                            labelSeparator: '',
                        },
                    ]
                },
                {
                    xtype: 'panel',
                    id: 'PanelP25',
                    border: false,
                    layout: 'hbox',
                    items: [
                        {
                            name: 'ORI_MMCODE1',
                            xtype: 'hidden',
                            value: 0
                        },
                        {
                            name: 'ORI_MMCODE2',
                            xtype: 'hidden',
                            value: 0
                        },
                        filterMMCode1,
                        filterMMCode2
                    ]
                },
                {
                    xtype: 'panel',
                    id: 'PanelP26',
                    border: false,
                    layout: 'hbox',
                    items: [
                        {
                            name: 'ORI_MMNAME',
                            xtype: 'hidden',
                            value: 0
                        },
                        {
                            xtype: 'textfield',
                            fieldLabel: '藥品名',
                            name: 'MMNAME',
                            enforceMaxLength: false,
                            labelSeparator: '',
                            
                        },
                    ]
                },
                {
                    xtype: 'panel',
                    id: 'PanelP23',
                    border: false,
                    layout: 'vbox',
                    items: [
                        {
                            name: 'ORI_GAP_T',  // 誤差量 
                            xtype: 'hidden',
                            value: 0
                        },
                        {
                            name: 'ORI_GAP_PRICE',  // 誤差金額
                            xtype: 'hidden',
                            value: 0
                        },
                        {
                            name: 'ORI_M_CONTPRICE',    // 進價
                            xtype: 'hidden',
                            value: 0
                        },
                        {
                            name: 'ORI_GAP_P',  // 誤差百分比
                            xtype: 'hidden',
                            value: 0
                        },
                        {
                            name: 'ORI_E_ORDERDCFLAG',  // 停用
                            xtype: 'hidden',
                            value: 'N'
                        },
                        {
                            name: 'ORI_DRUG_TYPE',  // 篩選條件
                            xtype: 'hidden',
                            value: ''
                        },
                        {
                            xtype: 'numberfield',
                            fieldLabel: '誤差量 >',
                            name: 'GAP_T',
                            enforceMaxLength: false,
                            maxLength: 14,
                            labelSeparator: '',
                            hideTrigger: true,
                            value: 0
                        },
                        {
                            xtype: 'numberfield',
                            fieldLabel: '誤差金額 >',
                            name: 'GAP_PRICE',
                            enforceMaxLength: false,
                            maxLength: 14,
                            labelSeparator: '',
                            hideTrigger: true,
                            value: 0
                        },
                        {
                            xtype: 'numberfield',
                            fieldLabel: '進價 >',
                            name: 'M_CONTPRICE',
                            enforceMaxLength: false,
                            maxLength: 14,
                            labelSeparator: '',
                            hideTrigger: true,
                            value: 0
                        },
                        {
                            xtype: 'numberfield',
                            fieldLabel: '誤差百分比 >',
                            name: 'GAP_P',
                            enforceMaxLength: false,
                            maxLength: 14,
                            labelSeparator: '',
                            hideTrigger: true,
                            value: 0
                        },
                        {
                            xtype: 'radiogroup',
                            name: 'E_ORDERDCFLAG',
                            fieldLabel: '停用',
                            items: [
                                { boxLabel: '否', name: 'E_ORDERDCFLAG', inputValue: 'N', width: 50, checked: true },
                                { boxLabel: '是', name: 'E_ORDERDCFLAG', inputValue: 'Y' }
                            ],
                            listeners: {
                                change: function (field, newValue, oldValue) {
                                    
                                    filter_e_orderdcflag = newValue.E_ORDERDCFLAG;
                                }
                            },
                            //labelWidth: 100,
                            //width: 250
                        },
                        {
                            xtype: 'combo',
                            store: drugtypeStore,
                            displayField: 'TEXT',
                            valueField: 'VALUE',
                            queryMode: 'local',
                            fieldLabel: '篩選條件',
                            name: 'DRUG_TYPE',
                            id: 'DRUG_TYPE',
                            enforceMaxLength: true,
                            padding: '0 4 0 4',
                            //width: 150
                            value: ''
                        }
                    ]
                },

            ]
        }]
    });

    var filterWindow = Ext.create('Ext.window.Window', {
        renderTo: Ext.getBody(),
        modal: true,
        items: [filterForm],
        //width: "900px",
        //height: windowHeight,
        resizable: false,
        draggable: false,
        closable: false,
        //x: ($(window).width() / 2) - 300,
        //y: 0,
        title: "篩選條件設定",
        //listeners: {
        //    close: function (panel, eOpts) {
        //        myMask.hide();
        //    }
        //}
        buttons: [
            {
                text: '確定',
                handler: function () {

                    var f = filterForm.getForm();
                    if (f.findField('GAP_T').getValue() == null) {
                        f.findField('GAP_T').setValue(0);
                    }
                    if (f.findField('GAP_PRICE').getValue() == null) {
                        f.findField('GAP_PRICE').setValue(0);
                    }
                    if (f.findField('M_CONTPRICE').getValue() == null) {
                        f.findField('M_CONTPRICE').setValue(0);
                    }
                    if (f.findField('GAP_P').getValue() == null) {
                        f.findField('GAP_P').setValue(0);
                    }

                    f.findField('ORI_GAP_T').setValue(f.findField('GAP_T').getValue());
                    f.findField('ORI_GAP_PRICE').setValue(f.findField('GAP_PRICE').getValue());
                    f.findField('ORI_M_CONTPRICE').setValue(f.findField('M_CONTPRICE').getValue());
                    f.findField('ORI_GAP_P').setValue(f.findField('GAP_P').getValue());
                    f.findField('ORI_E_ORDERDCFLAG').setValue(filter_e_orderdcflag);
                    f.findField('ORI_DRUG_TYPE').setValue(f.findField('DRUG_TYPE').getValue());
                    f.findField('ORI_SEQ1').setValue(f.findField('SEQ1').getValue());
                    f.findField('ORI_SEQ2').setValue(f.findField('SEQ2').getValue());
                    f.findField('ORI_MMCODE1').setValue(f.findField('MMCODE1').getValue());
                    f.findField('ORI_MMCODE2').setValue(f.findField('MMCODE2').getValue());
                    f.findField('ORI_MMNAME').setValue(f.findField('MMNAME').getValue());

                    T21Store.getProxy().setExtraParam("gap_t", f.findField('GAP_T').getValue());
                    T21Store.getProxy().setExtraParam("gap_price", f.findField('GAP_PRICE').getValue());
                    T21Store.getProxy().setExtraParam("m_contprice", f.findField('M_CONTPRICE').getValue());
                    T21Store.getProxy().setExtraParam("gap_p", f.findField('GAP_P').getValue());
                    
                    T21Store.getProxy().setExtraParam("e_orderdcflag", filter_e_orderdcflag);
                    T21Store.getProxy().setExtraParam("drug_type", f.findField('DRUG_TYPE').getValue());
                    T21Store.getProxy().setExtraParam("seq1", f.findField('SEQ1').getValue());
                    T21Store.getProxy().setExtraParam("seq2", f.findField('SEQ2').getValue());
                    T21Store.getProxy().setExtraParam("mmcode1", f.findField('MMCODE1').getValue());
                    T21Store.getProxy().setExtraParam("mmcode2", f.findField('MMCODE2').getValue());
                    T21Store.getProxy().setExtraParam("mmname", f.findField('MMNAME').getValue());

                    T21Load();

                    filterWindow.hide();
                }
            },
            {
                text: '取消',
                handler: function () {
                    var f = filterForm.getForm();
                    if (f.findField('GAP_T').getValue() == null) {
                        f.findField('GAP_T').setValue(0);
                    }
                    if (f.findField('GAP_PRICE').getValue() == null) {
                        f.findField('GAP_PRICE').setValue(0);
                    }
                    if (f.findField('M_CONTPRICE').getValue() == null) {
                        f.findField('M_CONTPRICE').setValue(0);
                    }
                    if (f.findField('GAP_P').getValue() == null) {
                        f.findField('GAP_P').setValue(0);
                    }

                    f.findField('GAP_T').setValue(f.findField('ORI_GAP_T').getValue());
                    f.findField('GAP_PRICE').setValue(f.findField('ORI_GAP_PRICE').getValue());
                    f.findField('M_CONTPRICE').setValue(f.findField('ORI_M_CONTPRICE').getValue());
                    f.findField('GAP_P').setValue(f.findField('ORI_GAP_P').getValue());
                    f.findField('E_ORDERDCFLAG').setValue({ E_ORDERDCFLAG: f.findField('ORI_E_ORDERDCFLAG').getValue()});
                    f.findField('DRUG_TYPE').setValue(f.findField('ORI_DRUG_TYPE').getValue());
                    f.findField('SEQ1').setValue(f.findField('ORI_SEQ1').getValue());
                    f.findField('SEQ2').setValue(f.findField('ORI_SEQ2').getValue());
                    f.findField('MMCODE1').setValue(f.findField('ORI_MMCODE1').getValue());
                    f.findField('MMCODE2').setValue(f.findField('ORI_MMCODE2').getValue());
                    f.findField('MMNAME').setValue(f.findField('ORI_MMNAME').getValue());

                    filterWindow.hide();
                }
            }
            //{
            //text: '關閉',
            //handler: function () {
            //    filterWindow.hide();
            //}
            //}
        ]
    });
    filterWindow.hide();
    //#endregion

    // #endregion --------------------------

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
                itemId: 't1Grid',
                region: 'center',
                layout: 'fit',
                collapsible: false,
                title: '',
                border: false,
                items: [T1Grid]
            },
        ]
    });

    Ext.on('resize', function () {
        windowWidth = $(window).width();
        windowHeight = $(window).height();
        detailWindow.setHeight(windowHeight);
        T21Grid.setHeight(windowHeight / 2 - 62);
        T22Grid.setHeight(windowHeight / 2 - 60);
    });

    var myMask = new Ext.LoadMask(viewport, { msg: '處理中...' });
    myMask.hide();
    T1Load(true);
});