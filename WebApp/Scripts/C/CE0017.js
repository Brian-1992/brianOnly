Ext.Loader.setConfig({
    enabled: true,
    paths: {
        'WEBAPP': '/Scripts/app'
    }
});

Ext.require(['WEBAPP.utils.Common']);

Ext.onReady(function () {
    var T1Set = ''; // 新增/修改/刪除

    var T1RecLength = 0;
    var T1LastRec = null;
    var gloval = null; //CHK_NO
    var windowHeight = $(window).height();
    var windowWidth = $(window).width();

    Ext.override(Ext.grid.column.Column, { menuDisabled: true });
    var T2Set = '../../../api/CE0016/UpdateDetails';
    var currentChknos = '';
    var multiCheckList = [];
    var wh_kind = '';
    var todayDateString = '';
    var WhnoComboGet = '../../../api/CE0002/GetWhnoCombo';
    var reportMultiChknoUrl = '/Report/C/CE0016.aspx';
    var reportMultiMmcodeUrl = '/Report/C/CE0016_1.aspx';
    var reportMultiMmcodeWardUrl = '/Report/C/CE0016_2.aspx';

    var viewModel = Ext.create('WEBAPP.store.CE0016VM');

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
                    var wh_nos = data.etts;
                    if (wh_nos.length > 0) {
                        for (var i = 0; i < wh_nos.length; i++) {
                            whnoQueryStore.add(wh_nos[i]);
                        }

                        T1Query.getForm().findField('P0').setValue(wh_nos[0].WH_NO);
                        T1Load();
                    }
                }
            },
            failure: function (response, options) {

            }
        });
    }

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

    // 查詢欄位
    var mLabelWidth = 90;
    var mWidth = 230;
    var T1Query = Ext.widget({
        itemId: 'queryform1',
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
            xtype: 'panel',
            id: 'PanelP2',
            border: false,
            layout: 'hbox',
            width:'100%',
            items: [
                //{
                //    xtype: 'button',
                //    text: '手機板',
                //    handler: function () {
                //        //location.href = "../../../Form/Index/CE0006";
                //        parent.link2('/Form/Index/CE0006', '複盤數量輸入作業CE0006', true);
                //    }
                //},
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
                    width: 230,
                    multiSelect: false,
                    fieldCls: 'required',
                    tpl: '<tpl for="."><div class="x-boundlist-item" style="height:auto;">{WH_NAME}&nbsp;</div></tpl>',
                },
                {
                    xtype: 'monthfield',
                    fieldLabel: '盤點年月',
                    name: 'D0',
                    id: 'D0',
                    labelWidth: 60,
                    width: 160,
                    fieldCls: 'required',
                    value: getDefaultValue()
                },
                {
                    xtype: 'displayfield',
                    fieldLabel: '盤點人員',
                    name: 'peop',
                    id: 'peop',
                    labelWidth: 60,
                    width: 180,
                    value: session['UserName']
                },
                {
                    xtype: 'button',
                    text: '查詢',
                    handler: function () {
                        if (T1Query.getForm().findField('P0').getValue() == null
                            || T1Query.getForm().findField('P0').getValue() == undefined
                            || T1Query.getForm().findField('P0').getValue() == '') {
                            Ext.Msg.alert('提醒', '<span style=\'color:red\'>庫房代碼</span>為必填');
                            return;
                        }

                        T1Load();

                    }
                }, {
                    xtype: 'button',
                    text: '清除',
                    handler: function () {
                        if (T1Query.getForm().findField('P0').getValue() == null
                            || T1Query.getForm().findField('P0').getValue() == undefined
                            || T1Query.getForm().findField('P0').getValue() == '') {
                            Ext.Msg.alert('提醒', '<span style=\'color:red\'>庫房代碼</span>為必填');
                            return;
                        }
                        if (T1Query.getForm().findField('D0').getValue() == null
                            || T1Query.getForm().findField('D0').getValue() == undefined
                            || T1Query.getForm().findField('D0').getValue() == '') {
                            Ext.Msg.alert('提醒', '<span style=\'color:red\'>盤點年月</span>為必填');
                            return;
                        }

                        T1Load();

                    }
                },
                {
                    xtype: 'component',
                    flex: 1
                },
                {
                    xtype: 'container',
                    layout: 'hbox',
                    right: '100%',
                    items: [
                        {
                            xtype: 'button',
                            text: '複盤確認',
                            handler: function () {
                                parent.link2('/Form/Index/CE0007', ' 複盤確認(CE0007)', true);
                                //parent.link2('/Form/Index/CE0016','初盤數量輸入作業');
                            }
                        }
                    ]
                }
            ]
        }]
    });
    function getDefaultValue() {
        var yyyy = 0;
        var m = 0;
        yyyy = new Date().getFullYear() - 1911;
        m = new Date().getMonth() + 1;
        var mm = m >= 10 ? m.toString() : "0" + m;
        //var mm = m >= 10 ? m.toString() : "0" + m.toString();
        return yyyy.toString() + mm;
    }

    var T1Store = viewModel.getStore('Masters');
    T1Store.on('beforeload', function (store, options) {

        T1Store.getProxy().setExtraParam('chk_level', '2');
        T1Store.getProxy().setExtraParam('wh_no', T1Query.getForm().findField('P0').getValue());
        T1Store.getProxy().setExtraParam('chk_ym', T1Query.getForm().findField('D0').rawValue);
    });

    function T1Load() {
        msglabel('');
        if (T1Query.getForm().findField('P0').getValue() == null || T1Query.getForm().findField('D0').getValue() == null) {
            Ext.Msg.alert('提醒', '<span style=\'color:red\'>庫房代碼</span>與<span style=\'color:red\'>盤點年月</span>為必填');
            return;
        }
        T1Tool.moveFirst();
    }

    // toolbar 
    var T1Tool = Ext.create('Ext.PagingToolbar', {
        store: T1Store, //資料load進來
        displayInfo: true,
        border: false,
        plain: true,
        buttons: [

            {
                itemId: 'FinalProID',
                text: "多筆盤點",
                id: 'btnMultiChk',
                disabled: false,
                handler: function () {
                    var selection = T1Grid.getSelection();

                    for (var i = 0; i < selection.length; i++) {
                        if (selection[i].data.CHK_STATUS_CODE != '1') {
                            Ext.Msg.alert('提示', '狀態為「盤中」才可進行多筆盤點');
                            return;
                        }
                    }
                    var isCE = true;
                    var list = [];
                    for (var i = 0; i < selection.length; i++) {
                        var item = selection[i].data;
                        wh_kind = item.CHK_WH_KIND_CODE;
                        if (item.CHK_WH_KIND_CODE != 'C' && item.CHK_WH_KIND_CODE != 'E') {
                            isCE = false;
                        }
                        list.push(item);
                    }
                    var noUser = '';

                    var chk_nos = '';
                    for (var i = 0; i < selection.length; i++) {
                        if (selection[i].data.CHK_KEEPER == '') {

                            if (noUser.length > 0) {
                                noUser += ',';
                            }
                            noUser += ("'" + selection[i].data.CHK_NO + "'");
                        }

                        if (chk_nos.length > 0) {
                            chk_nos += ',';
                        }
                        chk_nos += ("'" + selection[i].data.CHK_NO + "'");
                    }

                    currentChknos = chk_nos;
                    multiCheckList = list;

                    var index2_pre_inv_qty = findColumnIndex(T2Grid.columns, 'PRE_INV_QTY');
                    var index2_aplinqty = findColumnIndex(T2Grid.columns, 'APL_INQTY');
                    var index2_aploutqty = findColumnIndex(T2Grid.columns, 'APL_OUTQTY');
                    var index2_store_qty = findColumnIndex(T2Grid.columns, 'STORE_QTYC');
                    var index2_use_qty = findColumnIndex(T2Grid.columns, 'USE_QTY');
                    var index3_pre_inv_qty = findColumnIndex(T3Grid.columns, 'PRE_INV_QTY');
                    var index3_aplinqty = findColumnIndex(T3Grid.columns, 'APL_INQTY');
                    var index3_aploutqty = findColumnIndex(T3Grid.columns, 'APL_OUTQTY');
                    var index3_store_qty = findColumnIndex(T3Grid.columns, 'STORE_QTYC');
                    var index3_use_qty = findColumnIndex(T3Grid.columns, 'USE_QTY');

                    if (multiCheckList[0].CHK_WH_KIND_CODE == 'E' || multiCheckList[0].CHK_WH_KIND_CODE == 'C') {
                        T2Grid.getColumns()[index2_pre_inv_qty + 1].hide();
                        T2Grid.getColumns()[index2_aplinqty + 1].hide();
                        T2Grid.getColumns()[index2_aploutqty + 1].hide();
                        T2Grid.getColumns()[index2_use_qty + 1].hide();
                        T2Grid.getColumns()[index2_store_qty + 1].show();

                        T3Grid.getColumns()[index3_pre_inv_qty + 1].hide();
                        T3Grid.getColumns()[index3_aplinqty + 1].hide();
                        T3Grid.getColumns()[index3_aploutqty + 1].hide();
                        T3Grid.getColumns()[index3_use_qty + 1].hide();
                        T3Grid.getColumns()[index3_store_qty + 1].show();
                    } else {
                        T2Grid.getColumns()[index2_pre_inv_qty + 1].show();
                        T2Grid.getColumns()[index2_aplinqty + 1].show();
                        T2Grid.getColumns()[index2_aploutqty + 1].show();
                        T2Grid.getColumns()[index2_use_qty + 1].show();
                        T2Grid.getColumns()[index2_store_qty + 1].hide();

                        T3Grid.getColumns()[index3_pre_inv_qty + 1].show();
                        T3Grid.getColumns()[index3_aplinqty + 1].show();
                        T3Grid.getColumns()[index3_aploutqty + 1].show();
                        T3Grid.getColumns()[index3_use_qty + 1].show();
                        T3Grid.getColumns()[index3_store_qty + 1].hide();
                    }

                    T3Load(chk_nos, true);


                    if (isCE == false) {
                        Ext.getCmp('matchMulti').show();
                        Ext.getCmp('matchMultiCE').hide();
                    } else {
                        Ext.getCmp('matchMulti').hide();
                        Ext.getCmp('matchMultiCE').show();
                    }

                    //#region 2021-12-15 僅系統開立之日盤單顯示全數符合按鈕

                    var showMatchBtn = true;
                    for (var i = 0; i < selection.length; i++) {
                        if (selection[i].data.CHK_PERIOD_CODE != "D") {
                            showMatchBtn = false;
                        }
                        if (selection[i].data.CHK_WH_KIND_CODE != '0') {
                            showMatchBtn = false;
                        }
                        if (selection[i].data.CHK_NO1_CREATE_USER != 'BATCH') {
                            showMatchBtn = false;
                        }
                    }
                    if (showMatchBtn) {
                        Ext.getCmp('match').show();
                        Ext.getCmp('matchMulti').show();
                    } else {
                        Ext.getCmp('match').hide();
                        Ext.getCmp('matchMulti').hide();
                    }

                    //#endregion

                    multiCheckWindow.setTitle("複盤數量輸入作業: " + session['UserName']);

                    multiCheckWindow.show();//呼叫第二畫面

                    //setFinalData();
                }
            },
            {
                itemId: 'FinalPrint',
                text: "多筆列印",
                id: 'btnMultiPrint',
                disabled: false,
                handler: function () {
                    var selection = T1Grid.getSelection();

                    if (selection.length == 0) {
                        Ext.Msg.alert('提醒', '請選擇要列印的盤點單');
                        return;
                    }

                    for (var i = 0; i < selection.length; i++) {
                        if (Number(selection[i].data.CHK_STATUS) < 1) {
                            Ext.Msg.alert('提醒', '盤點單號：' + selection[i].data.CHK_NO + ' 狀態為準備中，不可列印');
                            return;
                        }
                    }

                    multiPrintWindow.show();
                }
            }
        ]
    });

    function showReport(url, chk_nos) {
        if (!win) {
            var winform = Ext.create('Ext.form.Panel', {
                id: 'iframeReport',
                //height: '100%',
                //width: '100%',
                layout: 'fit',
                closable: false,
                html: '<iframe src="' + url + '?chk_nos=' + chk_nos
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

    // 查詢結果資料列表
    var T1Grid = Ext.create('Ext.grid.Panel', {
        store: T1Store, //資料load進來
        plain: true,
        loadingText: '處理中...',
        loadMask: true,
        cls: 'T1',
        dockedItems: [{
            dock: 'top',
            xtype: 'toolbar',
            layout: 'fit',
            items: [T1Query]
        }, {
            dock: 'top',
            xtype: 'toolbar',
            items: [T1Tool]
            }],
        selModel: {
            checkOnly: false,
            injectCheckbox: 'first',
            mode: 'MULTI'
        },
        selType: 'checkboxmodel',
        // grid columns
        columns: {
            items: [{
                xtype: 'rownumberer',
                width: 30
            }, {
                text: "庫房代碼",
                dataIndex: 'CHK_WH_NO',
                style: 'text-align:left',
                align: 'left',
                width: 80
            }, {
                text: "庫房名稱",
                dataIndex: 'WH_NAME',
                width: 120
            }, {
                text: "盤點日期",
                dataIndex: 'CHK_YM',
                width: 60,
            }, {
                text: "庫別級別",
                dataIndex: 'CHK_WH_GRADE',
                width: 60,
            }, {
                text: "庫別分類",
                dataIndex: 'CHK_WH_KIND',
                width: 60
            }, {
                text: "盤點期",
                dataIndex: 'CHK_PERIOD',
                width: 60,

            }, {
                text: "盤點類別",
                dataIndex: 'CHK_TYPE',
                width: 80,
            }, {
                text: "盤點數量/盤點總量",
                dataIndex: 'MERGE_NUM_TOTAL',
                style: 'text-align:left',
                align: 'right',
                width: 135,
            }, {
                text: "盤點單號",
                dataIndex: 'CHK_NO',
                style: 'text-align:left',
                align: 'left',
                width: 120,
                renderer: function (val, meta, record) {
                    if (val != null) {
                        return '<a href=javascript:void(0)>' + val + '</a>';
                    }
                }
            },
            //{
            //    text: "負責人員",
            //    dataIndex: 'CHK_KEEPER',
            //    style: 'text-align:left',
            //    align: 'left',
            //    width: 80,
            //},
            {
                text: "狀態",
                dataIndex: 'CHK_STATUS',
                style: 'text-align:left',
                align: 'left',
                width: 60,
            }, {
                header: "",
                flex: 1
            }]
        },
        viewConfig: {
            listeners: {
                selectionchange: function (model, records) {
                    T1LastRec = records[0];
                },
                cellclick: function (view, cell, cellIndex, record, row, rowIndex, e) {
                    //
                    T1LastRec = record;

                    var columnIndex = view.getHeaderCt().getHeaderAtIndex(cellIndex).config.dataIndex;
                    if (columnIndex != 'CHK_NO') {
                        return;
                    }

                    var clickedDataIndex = view.panel.headerCt.getHeaderAtIndex(cellIndex).dataIndex;
                    var clickedColumnName = view.panel.headerCt.getHeaderAtIndex(cellIndex).text;
                    var clickedCellValue = record.get(clickedDataIndex); //得到值 clickedCellValue, CHK_NO
                    gloval = clickedCellValue;

                    wh_kind = T1LastRec.data.CHK_WH_KIND_CODE;
                    if (T1LastRec.data.CHK_WH_KIND_CODE != 'E' && T1LastRec.data.CHK_WH_KIND_CODE != 'C') {
                        Ext.getCmp('match').show();
                        Ext.getCmp('matchCE').hide();
                    } else {
                        Ext.getCmp('match').hide();
                        Ext.getCmp('matchCE').show();
                    }

                    var index2_pre_inv_qty = findColumnIndex(T2Grid.columns, 'PRE_INV_QTY');
                    var index2_aplinqty = findColumnIndex(T2Grid.columns, 'APL_INQTY');
                    var index2_aploutqty = findColumnIndex(T2Grid.columns, 'APL_OUTQTY');
                    var index2_store_qty = findColumnIndex(T2Grid.columns, 'STORE_QTYC');
                    var index2_use_qty = findColumnIndex(T2Grid.columns, 'USE_QTY');
                    var index3_pre_inv_qty = findColumnIndex(T3Grid.columns, 'PRE_INV_QTY');
                    var index3_aplinqty = findColumnIndex(T3Grid.columns, 'APL_INQTY');
                    var index3_aploutqty = findColumnIndex(T3Grid.columns, 'APL_OUTQTY');
                    var index3_store_qty = findColumnIndex(T3Grid.columns, 'STORE_QTYC');
                    var index3_use_qty = findColumnIndex(T3Grid.columns, 'USE_QTY');
                    
                    if (T1LastRec.data.CHK_WH_KIND_CODE == 'E' || T1LastRec.data.CHK_WH_KIND_CODE == 'C') {
                        T2Grid.getColumns()[index2_pre_inv_qty + 1].hide();
                        T2Grid.getColumns()[index2_aplinqty + 1].hide();
                        T2Grid.getColumns()[index2_aploutqty + 1].hide();
                        T2Grid.getColumns()[index2_aploutqty + 1].hide();
                        T2Grid.getColumns()[index2_use_qty + 1].show();

                        T3Grid.getColumns()[index3_pre_inv_qty + 1].hide();
                        T3Grid.getColumns()[index3_aplinqty + 1].hide();
                        T3Grid.getColumns()[index3_aploutqty + 1].hide();
                        T3Grid.getColumns()[index3_use_qty + 1].hide();
                        T3Grid.getColumns()[index3_store_qty + 1].show();
                    } else {
                        T2Grid.getColumns()[index2_pre_inv_qty + 1].show();
                        T2Grid.getColumns()[index2_aplinqty + 1].show();
                        T2Grid.getColumns()[index2_aploutqty + 1].show();
                        T2Grid.getColumns()[index2_aploutqty + 1].show();
                        T2Grid.getColumns()[index2_store_qty + 1].hide();

                        T3Grid.getColumns()[index3_pre_inv_qty + 1].show();
                        T3Grid.getColumns()[index3_aplinqty + 1].show();
                        T3Grid.getColumns()[index3_aploutqty + 1].show();
                        T3Grid.getColumns()[index3_use_qty + 1].show();
                        T3Grid.getColumns()[index3_store_qty + 1].hide();
                    }

                    //#region 2021-12-15 僅系統開立之日盤單顯示全數符合按鈕

                    var showMatchBtn = true;
                    if (T1LastRec.data.CHK_PERIOD_CODE != "D") {
                        showMatchBtn = false;
                    }
                    if (T1LastRec.data.CHK_WH_KIND_CODE != '0') {
                        showMatchBtn = false;
                    }
                    if (T1LastRec.data.CHK_NO1_CREATE_USER != 'BATCH') {
                        showMatchBtn = false;
                    }
                    if (showMatchBtn) {
                        Ext.getCmp('match').show();
                        Ext.getCmp('matchMulti').show();
                    } else {
                        Ext.getCmp('match').hide();
                        Ext.getCmp('matchMulti').hide();
                    }

                    //#endregion

                    T2Load();//T2Grid更新

                    popformINI.setTitle("複盤數量輸入作業: " + T1LastRec.data.CHK_WH_NO + " " + T1LastRec.data.CHK_YM + " " + T1LastRec.data.CHK_PERIOD + " " + T1LastRec.data.CHK_TYPE +
                        " " + T1LastRec.data.CHK_STATUS + " " + T1LastRec.data.CHK_WH_KIND + " " + T1LastRec.data.CHK_NO + " " + session['UserName']);
                    T2Tool.resumeEvents(); //T2Tool恢復beforeChange功能
                    popformINI.show();//呼叫第二畫面
                }
            }
        }
    });
    var findColumnIndex = function (columns, dataIndex) {
        var index;
        for (index = 0; index < columns.length; ++index) {
            if (columns[index].dataIndex == dataIndex) { break; }
        }
        return index == columns.length ? -1 : index;
    }

    var T2Store = viewModel.getStore('Details');
    T2Store.on('beforeload', function (store, options) {

        T2Store.getProxy().setExtraParam('chk_no', gloval);
        T2Store.getProxy().setExtraParam('mmcode', T2Query.getForm().findField('MMCODE').getValue());
        T2Store.getProxy().setExtraParam('chk_time_status', T2Query.getForm().findField('chk_time_status').getValue());

    });
    T2Store.on('load', function (store, records) {
        if (T1LastRec == null) {
            T2Grid.down('#update').setDisabled(true);
            T2Grid.down('#FinalProID').setDisabled(true);
            T2Grid.down('#match').setDisabled(true);
            return;
        }

        if (T1LastRec.data.CHK_STATUS_CODE == "1") {
            if (isEditable(T1LastRec.data.CHK_YM)) {
                Ext.getCmp('update').enable();
                Ext.getCmp('match').enable();
                Ext.getCmp('matchCE').enable();
                Ext.getCmp('FinalProID').enable();
            } else {
                Ext.getCmp('update').disable();
                Ext.getCmp('match').disable();
                Ext.getCmp('matchCE').disable();
                Ext.getCmp('FinalProID').disable();
            }
        } else {
            T2Grid.down('#update').setDisabled(true);
            T2Grid.down('#FinalProID').setDisabled(true);
            T2Grid.down('#match').setDisabled(true);
        }
    });
    
    function T2Load() {

        T2Tool.moveFirst();
        msglabel('訊息區:');
    }

    var T2Tool = Ext.create('Ext.PagingToolbar', {
        store: T2Store,
        displayInfo: true,
        border: false,
        plain: true,
        buttons: [
            {
                id: 'update',
                itemId: 'update', text: '儲存', disabled: true, handler: function () {
                    
                    var tempData = T2Grid.getStore().data.items;
                    var data = [];
                    let CHK_QTY = '';
                    let CHK_REMARK = '';
                    for (var i = 0; i < tempData.length; i++) {
                        if (tempData[i].dirty) {
                            data.push(tempData[i].data);
                        }
                    }

                    var myMask = new Ext.LoadMask(viewport, { msg: '處理中...' });
                    myMask.show();

                    var url = '../../../api/CE0016/UpdateDetails';
                    if (T1LastRec.data.CHK_WH_KIND_CODE == 'E' || T1LastRec.data.CHK_WH_KIND_CODE == 'C') {
                        url = '../../../api/CE0016/UpdateDetailsCE'
                    }

                    Ext.Ajax.request({
                        url: url,
                        method: reqVal_p,
                        contentType: "application/json",
                        params: { list: Ext.util.JSON.encode(data), wh_kind: wh_kind },
                        success: function (response) {
                            var data = Ext.decode(response.responseText);
                            myMask.hide();
                            if (data.success) {
                                msglabel('訊息區:資料更新成功');
                                T2Store.load({
                                    params: {
                                        start: 0
                                    }
                                });
                            } else {
                                Ext.Msg.alert('失敗', 'Ajax communication failed');
                            }
                        },
                        failure: function (response, action) {
                            myMask.hide();
                            Ext.Msg.alert('失敗', 'Ajax communication failed');
                        }
                    });
                }
            },
            {
                id: 'match',
                itemId: 'match',
                text: "全數符合",
                disabled: false,
                handler: function () {
                    var selection = T2Grid.getSelection();

                    if (selection.length == 0) {
                        Ext.Msg.alert('提示', '請選擇項目');
                        return;
                    }

                    var list = [];
                    for (var i = 0; i < selection.length; i++) {
                        var data = selection[i].data;
                        list.push(data);
                    }

                    Ext.Ajax.request({
                        url: '/api/CE0016/Match',
                        method: reqVal_p,
                        params: {
                            list: Ext.util.JSON.encode(list)
                        },
                        success: function (response) {
                            msglabel('訊息區:資料更新成功');
                            T2Store.load({
                                params: {
                                    start: 0
                                }
                            });
                        },
                        failure: function (response, options) {
                            Ext.Msg.alert('失敗', '發生例外錯誤');
                        }
                    });
                }
            },
            {
                id: 'matchCE',
                itemId: 'matchCE',
                text: "全數符合'",
                disabled: false,
                //hidden:true,
                handler: function () {

                    Ext.Ajax.request({
                        url: '/api/CE0016/MatchCE',
                        method: reqVal_p,
                        params: {
                            chk_no: "'" + T1LastRec.data.CHK_NO + "'",
                            wh_kind: wh_kind
                        },
                        success: function (response) {
                            msglabel('訊息區:資料更新成功');
                            T2Store.load({
                                params: {
                                    start: 0
                                }
                            });
                        },
                        failure: function (response, options) {
                            Ext.Msg.alert('失敗', '發生例外錯誤');
                        }
                    });
                }
            },
            {
                id: 'FinalProID',
                itemId: 'FinalProID',
                text: "<span style='color:red'>完成盤點</span>",
                disabled: false,
                handler: function () {
                    setFinalData();
                }
            }
        ]
    });

    //完成盤點
    function setFinalData() {
        //先儲存
        var tempData = T2Grid.getStore().data.items;
        var datas = [];
        let CHK_QTY = '';
        let CHK_REMARK = '';
        for (var i = 0; i < tempData.length; i++) {
            if (tempData[i].dirty) {
                datas.push(tempData[i].data);
            }
        }

        var myMask = new Ext.LoadMask(viewport, { msg: '處理中...' });
        myMask.show();

        var url = '../../../api/CE0016/UpdateDetails';
        if (T1LastRec.data.CHK_WH_KIND_CODE == 'E' || T1LastRec.data.CHK_WH_KIND_CODE == 'C') {
            url = '../../../api/CE0016/UpdateDetailsCE'
        }
        
        Ext.Ajax.request({
            url: url,
            method: reqVal_p,
            contentType: "application/json",
            params: { list: Ext.util.JSON.encode(datas), wh_kind: wh_kind },
            success: function (response) {
                var data = Ext.decode(response.responseText);
                myMask.hide();
                if (data.success) {
                    T2Load();
                    Ext.Ajax.request({
                        url: '/api/CE0016/FinishAll',
                        method: reqVal_p,
                        params: { chk_nos: gloval },
                        success: function (response) {
                            var data = Ext.decode(response.responseText);

                            if (data.success) {
                                var tb_msg = data.msg;
                                var tb_etts = data.etts;
                                var tips = "";
                                if (tb_msg == "尚有品項未盤點!") {  //尚有負責的品項未盤點
                                    Ext.Msg.alert('提醒', '尚有品項未盤點!');
                                } else if (tb_msg == "您已經完成此單號的盤點!") {   //做update
                                    T1Load();                                      //更新主畫面
                                    popformINI.hide();
                                    multiCheckWindow.hide();
                                    msglabel('已完成此單號的盤點輸入');
                                }
                            } else {
                                Ext.Msg.alert('提醒', data.msg);
                            }
                        },
                        failure: function (response, options) {

                        }
                    });
                } else {
                    Ext.Msg.alert('失敗', 'Ajax communication failed');
                }
            },

            failure: function (response, action) {
                myMask.hide();
                Ext.Msg.alert('失敗', 'Ajax communication failed');
            }
        });
    }

    var chkStatusStore = Ext.create('Ext.data.Store', {
        fields: ['VALUE', 'TEXT'],
        data: [
            { VALUE: '', TEXT: '全部' },
            { VALUE: 'is', TEXT: '未盤點' },
            { VALUE: 'is not', TEXT: '已盤點' },
        ]
    });
    var T2Query = Ext.widget({
        itemId: 'queryform2',
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
                xtype: 'panel',
                id: 'PanelP21',
                border: false,
                layout: 'hbox',
                width: '100%',
                items: [
                    {
                        xtype: 'textfield',
                        fieldLabel: '院內碼',
                        name: 'MMCODE',
                        enforceMaxLength: false,
                        maxLength: 13,
                    },
                    {
                        xtype: 'combo',
                        store: chkStatusStore,
                        name: 'chk_time_status',
                        id: 'chk_time_status',
                        fieldLabel: '盤點狀態',
                        displayField: 'TEXT',
                        valueField: 'VALUE',
                        queryMode: 'local',
                        anyMatch: true,
                        allowBlank: true,
                        typeAhead: true,
                        forceSelection: true,
                        width: 230,
                        triggerAction: 'all',
                        multiSelect: false,
                        tpl: '<tpl for="."><div class="x-boundlist-item" style="height:auto;">{TEXT}&nbsp;</div></tpl>',
                        value: ''
                    },
                    {
                        xtype: 'button',
                        text: '查詢',
                        handler: T2Load
                    }, {
                        xtype: 'button',
                        text: '清除',
                        handler: function () {
                            var f = this.up('form').getForm();
                            f.reset();
                            msglabel('訊息區:');
                        }
                    }
                ]
            }]
    });
    var T2Grid = Ext.create('Ext.grid.Panel', {
        store: T2Store, //資料load進來
        plain: true,
        loadingText: '處理中...',
        loadMask: true,
        cls: 'T2',
        dockedItems: [{
            dock: 'top',
            xtype: 'toolbar',
            layout: 'fit',
            items: [T2Query]
        },
            {
            dock: 'top',
            xtype: 'toolbar',
            items: [T2Tool]
        }],
        enableKeyEvents: true,
        selModel: Ext.create('Ext.selection.CheckboxModel', {
            checkOnly: true,
            injectCheckbox: 'first',
            mode: 'SIMPLE',
            showHeaderCheckbox: true
        }),
        columns: [{
            xtype: 'rownumberer'
        }, {
            text: "院內碼",
            dataIndex: 'MMCODE',
            width: 120,
            renderer: function (value, metaData, record, rowIndex) {
                if (record.data.M_TRNID == '2') {
                    return '<span>' + value + '</span>';
                }

                if (record.data.STORE_QTYC != record.data.CHK_QTY && record.data.CHK_TIME != "" && record.data.CHK_TIME != null) {
                    //metaData.tdStyle = 'background:#FFC8B4;';
                    return '<span style="color:red">' + value + '</span>';
                } else {
                    return '<span>' + value + '</span>';
                }
            }
        }, {
            text: "中文品名",
            dataIndex: 'MMNAME_C',
            width: 200,
            renderer: function (value, metaData, record, rowIndex) {
                if (record.data.M_TRNID == '2') {
                    return '<span>' + value + '</span>';
                }
                if (record.data.STORE_QTYC != record.data.CHK_QTY && record.data.CHK_TIME != "" && record.data.CHK_TIME != null) {
                    //metaData.tdStyle = 'background:#FFC8B4;';
                    return '<span style="color:red">' + value + '</span>';
                } else {
                    return '<span>' + value + '</span>';
                }
            }
        }, {
            text: "英文品名",
            dataIndex: 'MMNAME_E',
            width: 200,
            renderer: function (value, metaData, record, rowIndex) {
                if (record.data.M_TRNID == '2') {
                    return '<span>' + value + '</span>';
                }
                if (record.data.STORE_QTYC != record.data.CHK_QTY && record.data.CHK_TIME != "" && record.data.CHK_TIME != null) {
                    //metaData.tdStyle = 'background:#FFC8B4;';
                    return '<span style="color:red">' + value + '</span>';
                } else {
                    return '<span>' + value + '</span>';
                }
            }
        }, {
            text: "儲位",
            dataIndex: 'STORE_LOC',
            width: 80,
            renderer: function (value, metaData, record, rowIndex) {

                if (record.data.CHK_TIME != null && record.data.CHK_TIME != "") {       //CHK_TIME不為0，則造舊
                } else if (record.data.CHK_WH_NO == 'PH1S' && record.data.CHK_TIME == null && record.data.CHK_QTY == null) { //表示身份是PH1S且CHK_TIME為空,CHK_QTY沒值
                    record.data.CHK_QTY = record.data.STORE_QTYC;
                }
                if (record.data.M_TRNID == '2') {
                    return '<span>' + value + '</span>';
                }

                if (record.data.STORE_QTYC != record.data.CHK_QTY && record.data.CHK_TIME != "" && record.data.CHK_TIME != null) {
                    //metaData.tdStyle = 'background:#FFC8B4;';
                    return '<span style="color:red">' + value + '</span>';
                } else {
                    return '<span>' + value + '</span>';
                }

            }
        }, {
            text: "儲位名稱",
            dataIndex: 'LOC_NAME',
            width: 80,
            renderer: function (value, metaData, record, rowIndex) {
                if (record.data.M_TRNID == '2') {
                    return '<span>' + value + '</span>';
                }

                if (record.data.STORE_QTYC != record.data.CHK_QTY && record.data.CHK_TIME != "" && record.data.CHK_TIME != null) {
                    //metaData.tdStyle = 'background:#FFC8B4;';
                    return '<span style="color:red">' + value + '</span>';
                } else {
                    return '<span>' + value + '</span>';
                }
            }
        }, {
            text: "計量單位",
            dataIndex: 'BASE_UNIT',
            width: 80,
            renderer: function (value, metaData, record, rowIndex) {
                if (record.data.M_TRNID == '2') {
                    return '<span>' + value + '</span>';
                }
                if (record.data.STORE_QTYC != record.data.CHK_QTY && record.data.CHK_TIME != "" && record.data.CHK_TIME != null) {
                    // metaData.tdStyle = 'background:#FFC8B4;';
                    return '<span style="color:red">' + value + '</span>';
                } else {
                    return '<span>' + value + '</span>';
                }
            }
        }, {
            text: "上期結存量",
            dataIndex: 'PRE_INV_QTY',
            align: 'right',
            style: 'text-align:left',
            width: 85,
            renderer: function (value, metaData, record, rowIndex) {
                if (record.data.M_TRNID == '2') {
                    return '<span>' + value + '</span>';
                }
                if (record.data.STORE_QTYC != record.data.CHK_QTY && record.data.CHK_TIME != "" && record.data.CHK_TIME != null) {
                    // metaData.tdStyle = 'background:#FFC8B4;';
                    return '<span style="color:red">' + value + '</span>';
                } else {
                    return '<span>' + value + '</span>';
                }
            }
        }, {
            text: "入庫量",
            dataIndex: 'APL_INQTY',
            align: 'right',
            style: 'text-align:left',
            width: 70,
            renderer: function (value, metaData, record, rowIndex) {
                if (record.data.M_TRNID == '2') {
                    return '<span>' + value + '</span>';
                }
                if (record.data.STORE_QTYC != record.data.CHK_QTY && record.data.CHK_TIME != "" && record.data.CHK_TIME != null) {
                    // metaData.tdStyle = 'background:#FFC8B4;';
                    return '<span style="color:red">' + value + '</span>';
                } else {
                    return '<span>' + value + '</span>';
                }
            }
        }, {
            text: "出庫量",
            dataIndex: 'APL_OUTQTY',
            align: 'right',
            style: 'text-align:left',
            width: 70,
            renderer: function (value, metaData, record, rowIndex) {
                if (record.data.M_TRNID == '2') {
                    return '<span>' + value + '</span>';
                }
                if (record.data.STORE_QTYC != record.data.CHK_QTY && record.data.CHK_TIME != "" && record.data.CHK_TIME != null) {
                    // metaData.tdStyle = 'background:#FFC8B4;';
                    return '<span style="color:red">' + value + '</span>';
                } else {
                    return '<span>' + value + '</span>';
                }
            }
        }, {
            text: "調帳入",
            dataIndex: 'ADJ_INQTY',
            align: 'right',
            style: 'text-align:left',
            width: 70,
            renderer: function (value, metaData, record, rowIndex) {
                if (record.data.M_TRNID == '2') {
                    return '<span>' + value + '</span>';
                }
                if (record.data.STORE_QTYC != record.data.CHK_QTY && record.data.CHK_TIME != "" && record.data.CHK_TIME != null) {
                    // metaData.tdStyle = 'background:#FFC8B4;';
                    return '<span style="color:red">' + value + '</span>';
                } else {
                    return '<span>' + value + '</span>';
                }
            }
        }, {
            text: "調帳出",
            dataIndex: 'ADJ_OUTQTY',
            align: 'right',
            style: 'text-align:left',
            width: 70,
            renderer: function (value, metaData, record, rowIndex) {
                if (record.data.M_TRNID == '2') {
                    return '<span>' + value + '</span>';
                }
                if (record.data.STORE_QTYC != record.data.CHK_QTY && record.data.CHK_TIME != "" && record.data.CHK_TIME != null) {
                    // metaData.tdStyle = 'background:#FFC8B4;';
                    return '<span style="color:red">' + value + '</span>';
                } else {
                    return '<span>' + value + '</span>';
                }
            }
        }, {
            text: "調撥入",
            dataIndex: 'TRN_INQTY',
            align: 'right',
            style: 'text-align:left',
            width: 70,
            renderer: function (value, metaData, record, rowIndex) {
                if (record.data.M_TRNID == '2') {
                    return '<span>' + value + '</span>';
                }
                if (record.data.STORE_QTYC != record.data.CHK_QTY && record.data.CHK_TIME != "" && record.data.CHK_TIME != null) {
                    // metaData.tdStyle = 'background:#FFC8B4;';
                    return '<span style="color:red">' + value + '</span>';
                } else {
                    return '<span>' + value + '</span>';
                }
            }
        }, {
            text: "調撥出",
            dataIndex: 'TRN_OUTQTY',
            align: 'right',
            style: 'text-align:left',
            width: 70,
            renderer: function (value, metaData, record, rowIndex) {
                if (record.data.M_TRNID == '2') {
                    return '<span>' + value + '</span>';
                }
                if (record.data.STORE_QTYC != record.data.CHK_QTY && record.data.CHK_TIME != "" && record.data.CHK_TIME != null) {
                    // metaData.tdStyle = 'background:#FFC8B4;';
                    return '<span style="color:red">' + value + '</span>';
                } else {
                    return '<span>' + value + '</span>';
                }
            }
        },
        {
            text: "批價扣庫",
            dataIndex: 'USE_QTY',
            align: 'right',
            style: 'text-align:left',
            width: 70,
            renderer: function (value, metaData, record, rowIndex) {
                if (record.data.M_TRNID == '2') {
                    return '<span>' + value + '</span>';
                }
                if (record.data.STORE_QTYC != record.data.CHK_QTY && record.data.CHK_TIME != "" && record.data.CHK_TIME != null) {
                    // metaData.tdStyle = 'background:#FFC8B4;';
                    return '<span style="color:red">' + value + '</span>';
                } else {
                    return '<span>' + value + '</span>';
                }
            }
        },
        {
            text: "電腦量",
            align: 'right',
            style: 'text-align:left',
            dataIndex: 'STORE_QTYC',
            width: 70,
            renderer: function (value, metaData, record, rowIndex) {
                if (record.data.M_TRNID == '2') {
                    return '<span>' + value + '</span>';
                }
                // metaData.style = 'beckground:yellow';
                if (record.data.STORE_QTYC != record.data.CHK_QTY && record.data.CHK_TIME != "" && record.data.CHK_TIME != null) {
                    //metaData.tdStyle = 'background:#FFC8B4;';
                    return '<span style="color:red">' + value + '</span>';
                } else {
                    return '<span>' + value + '</span>';
                }
            }
        },
        {
            text: "盤點量",
            style: 'text-align:left; color:red',
            align: 'right',
            dataIndex: 'CHK_QTY',
            width: 70,
            renderer: function (value, metaData, record, rowIndex) {
                if (record.data.M_TRNID == '2') {
                    return '<span>' + value + '</span>';
                }
                if (record.data.STORE_QTYC != record.data.CHK_QTY && record.data.CHK_TIME != "" && record.data.CHK_TIME != null) {
                    //metaData.tdStyle = 'background:#FFC8B4;';
                    return '<span style="color:red">' + value + '</span>';
                } else {
                    return '<span>' + value + '</span>';
                }
            },
            editor: {
                xtype: 'textfield',
                regexText: '只能輸入數字',
                regex: /^[0-9]+$/, // 用正規表示式限制可輸入內容
                selectOnFocus: true,
                listeners: {
                    change: function (field, newVal, oldVal) {
                    },
                }
            },
        }, {
            text: "盤差",
            align: 'right',
            style: 'text-align:left',
            dataIndex: 'DIFF_QTY',
            width: 70,
            renderer: function (value, metaData, record, rowIndex, cell, column) {
                if (record.data.M_TRNID == '2') {
                    return '<span>' + value + '</span>';
                }
                if (record.data.STORE_QTYC != record.data.CHK_QTY && record.data.CHK_TIME != "" && record.data.CHK_TIME != null) {
                    //metaData.tdStyle = 'background:#FFC8B4;';
                    return '<span style="color:red">' + value + '</span>';
                } else {
                    return '<span>' + value + '</span>';
                }
            }
        },
        {
            text: "盤點人員",
            //style: 'text-align:left; color:red',
            dataIndex: 'CHK_UID_NAME',
            width: 90,
            renderer: function (value, metaData, record, rowIndex) {
                if (record.data.M_TRNID == '2') {
                    return '<span>' + value + '</span>';
                }
                if (record.data.STORE_QTYC != record.data.CHK_QTY && record.data.CHK_TIME != "" && record.data.CHK_TIME != null) {
                    //metaData.tdStyle = 'background:#ffe1e1;';
                    return '<span style="color:red">' + value + '</span>';
                } else {
                    return '<span>' + value + '</span>';
                }
            }
        },
        {
            text: "盤點時間",
            dataIndex: 'CHK_TIME',
            width: 120,
            renderer: function (value, meta, record) {
                if (value == "Mon Jan 01 0001 00:00:00 GMT+0806 (Taipei Standard Time)") {  //value值是 null時候
                    return '';
                } else if (value == "Mon Jan 01 0001 00:00:00 GMT+0806 (台北標準時間)") {
                    return '';
                } else {
                    if (record.data.M_TRNID == '2') {
                        return '<span>' + Ext.util.Format.date(value, 'Xmd H:i:s') + '</span>';
                    }
                    if (record.data.STORE_QTYC != record.data.CHK_QTY && record.data.CHK_TIME != "" && record.data.CHK_TIME != null) {
                        return '<span style="color:red">' + Ext.util.Format.date(value, 'Xmd H:i:s') + '</span>';
                    } else {
                        return '<span>' + Ext.util.Format.date(value, 'Xmd H:i:s') + '</span>';
                    }
                }
            }
        },
        //{
        //    text: "扣庫方式",
        //    dataIndex: 'M_TRNID',
        //    width: 80,
        //    renderer: function (value, metaData, record, rowIndex) {
        //        var display = '扣庫';
        //        if (value == '2') {
        //            display = '不扣庫';
        //        }

        //        if (record.data.M_TRNID == '2') {
        //            return '<span>' + display + '</span>';
        //        }
        //        if (record.data.STORE_QTYC != record.data.CHK_QTY && record.data.CHK_TIME != "" && record.data.CHK_TIME != null) {
        //            //metaData.tdStyle = 'background:#ffe1e1;';
        //            return '<span style="color:red">' + display + '</span>';
        //        } else {
        //            return '<span>' + display + '</span>';
        //        }
        //    }
        //},
        {
            dataIndex: 'STATUS_INI',
            width: '10%',
            hidden: true
        },
        {
            header: "",
            flex: 1
        }],
        plugins: [
            Ext.create('Ext.grid.plugin.CellEditing', {
                clicksToEdit: 1
            })
        ],
        listeners: {
            beforeedit: function (editor, e) {
                if (isEditable(T1LastRec.data.CHK_YM) == false) {
                    return false;
                }

                var editColumnIndex = findColumnIndex(T2Grid.columns, 'CHK_QTY');
                // STATUS_INI不是1 則不可填寫
                if (e.colIdx === editColumnIndex && e.record.get('STATUS_INI') != '1') {
                    return false;
                } 
            }
        },
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
                    //    T2Load();
                    //}

                    //alert('The press key is' + e.getKey());
                }
            }
        },
    });

    var viewport = Ext.create('Ext.Viewport', {
        renderTo: body,
        layout: {
            type: 'fit',
            padding: 0
        },
        defaults: {
            split: true  //可以調整大小
        },
        items: [{
            itemId: 'T1Grid',
            region: 'center',
            layout: 'fit',
            split: true,
            collapsible: false,
            border: false,
            items: [T1Grid]
        }]
    });

    var popformINI = Ext.create('Ext.window.Window', {
        renderTo: Ext.getBody(),
        id: 'NewWindow',
        width: windowWidth,
        height: windowHeight,
        xtype: 'form',
        layout: 'form',
        closable: false,
        layout: {
            type: 'fit',
            padding: 0
        },
        defaults: {
            split: true  //可以調整大小
        },
        items: [{
            itemId: 't2Grid',
            region: 'center',
            layout: 'fit',
            split: true,
            collapsible: false,
            border: false,
            items: [T2Grid]
        }],
        buttons: [
            {
                id: 'backID',
                disabled: false,
                text: '關閉',
                handler: function () {
                    var tempData = T2Grid.getStore().data.items;
                    var if_dirty = "no";
                    for (var i = 0; i < tempData.length; i++) {
                        if (tempData[i].dirty) {
                            if_dirty = "yes";
                            break;
                        }
                    }
                    if (if_dirty == "no") {
                        this.up('window').hide();
                    } else {
                        Ext.MessageBox.confirm(
                            "提醒",
                            "您的資料未儲存!確定要回上一頁?",
                            function (btn) {
                                if (btn == "yes") {
                                    T2Tool.suspendEvents();
                                    var win = Ext.getCmp('NewWindow');
                                    win.hide();
                                }
                                else {
                                }
                            });
                    }
                }
            }]
    });


    //#region 多單盤點
    var T3Store = viewModel.getStore('DetailsMulti');
    function T3Load(chk_nos, ismoveFirst) {
        T3Store.getProxy().setExtraParam("chk_nos", chk_nos);
        T3Store.getProxy().setExtraParam("mmcode", T3Query.getForm().findField('MMCODE').getValue());
        T3Store.getProxy().setExtraParam("chk_time_status", T3Query.getForm().findField('chk_time_status').getValue());

        if (ismoveFirst) {
            T3Tool.moveFirst();
        } else {
            T3Store.load({
                params: {
                    start: 0
                }
            });
        }
    }
    T3Store.on('load', function (store, records) {
        
        if (isEditable(multiCheckList[0].CHK_YM)) {
            Ext.getCmp('updateMulti').enable();
            Ext.getCmp('matchMulti').enable();
            Ext.getCmp('matchMultiCE').enable();
            Ext.getCmp('FinalProIDMulti').enable();
        } else {
            Ext.getCmp('updateMulti').disable();
            Ext.getCmp('matchMulti').disable();
            Ext.getCmp('matchMultiCE').disable();
            Ext.getCmp('FinalProIDMulti').disable();
        }

    });
    //完成盤點
    function setFinalAll() {
        //先儲存
        
        var tempData = T3Grid.getStore().data.items;
        var datas = [];
        let CHK_QTY = '';
        let CHK_REMARK = '';
        for (var i = 0; i < tempData.length; i++) {
            if (tempData[i].dirty) {
                datas.push(tempData[i].data);
            }
        }

        var myMask = new Ext.LoadMask(viewport, { msg: '處理中...' });
        myMask.show();

        var url = '../../../api/CE0016/UpdateDetails';
        if (T1LastRec.data.CHK_WH_KIND_CODE == 'E' || T1LastRec.data.CHK_WH_KIND_CODE == 'C') {
            url = '../../../api/CE0016/UpdateDetailsCE'
        }

        Ext.Ajax.request({
            url: url,
            method: reqVal_p,
            contentType: "application/json",
            params: { list: Ext.util.JSON.encode(datas), wh_kind: wh_kind },
            success: function (response) {
                var data = Ext.decode(response.responseText);
                myMask.hide();
                if (data.success) {
                    T3Load(currentChknos, true);
                    Ext.Ajax.request({
                        url: '/api/CE0016/FinishAll',
                        method: reqVal_p,
                        params: { chk_nos: currentChknos },
                        success: function (response) {
                            var data = Ext.decode(response.responseText);

                            if (data.success) {
                                var tb_msg = data.msg;
                                var tb_etts = data.etts;
                                var tips = "";
                                if (tb_msg == "尚有負責的品項未盤點!") {  //尚有負責的品項未盤點
                                    Ext.Msg.alert('提醒', '尚有負責的品項未盤點!');
                                } else if (tb_msg == "您已經完成此單號的盤點!") {   //做update
                                    T1Load();                                      //更新主畫面
                                    multiCheckWindow.hide();
                                    msglabel(tb_msg);
                                }
                            }
                        },
                        failure: function (response, options) {

                        }
                    });
                } else {
                    Ext.Msg.alert('失敗', 'Ajax communication failed');
                }
            },

            failure: function (response, action) {
                myMask.hide();
                Ext.Msg.alert('失敗', 'Ajax communication failed');
            }
        });
    }
    var T3Query = Ext.widget({
        itemId: 'queryform3',
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
                xtype: 'panel',
                id: 'PanelP4',
                border: false,
                layout: 'hbox',
                width: '100%',
                items: [
                    {
                        xtype: 'textfield',
                        fieldLabel: '院內碼',
                        name: 'MMCODE',
                        enforceMaxLength: false,
                        maxLength: 13,
                    },
                    {
                        xtype: 'combo',
                        store: chkStatusStore,
                        name: 'chk_time_status',
                        fieldLabel: '盤點狀態',
                        displayField: 'TEXT',
                        valueField: 'VALUE',
                        queryMode: 'local',
                        anyMatch: true,
                        allowBlank: true,
                        typeAhead: true,
                        forceSelection: true,
                        width: 230,
                        triggerAction: 'all',
                        multiSelect: false,
                        tpl: '<tpl for="."><div class="x-boundlist-item" style="height:auto;">{TEXT}&nbsp;</div></tpl>',
                        value: ''
                    },
                    {
                        xtype: 'button',
                        text: '查詢',
                        handler: function () {
                            T3Load(currentChknos, true);
                        }
                    }, {
                        xtype: 'button',
                        text: '清除',
                        handler: function () {
                            var f = this.up('form').getForm();
                            f.reset();
                            msglabel('訊息區:');
                        }
                    }
                ]
            }]
    });
    var T3Tool = Ext.create('Ext.PagingToolbar', {
        store: T3Store,
        displayInfo: true,
        border: false,
        plain: true,
        listeners: {

        },
        buttons: [
            {
                id: 'updateMulti',
                itemId: 'updateMulti', text: '儲存', handler: function () {
                    
                    var tempData = T3Grid.getStore().data.items;
                    var data = [];
                    let CHK_QTY = '';
                    let CHK_REMARK = '';
                    for (var i = 0; i < tempData.length; i++) {
                        if (tempData[i].dirty) {
                            data.push(tempData[i].data);
                        }
                    }

                    var myMask = new Ext.LoadMask(viewport, { msg: '處理中...' });
                    myMask.show();

                    var url = '../../../api/CE0016/UpdateDetails';
                    if (T1LastRec.data.CHK_WH_KIND_CODE == 'E' || T1LastRec.data.CHK_WH_KIND_CODE == 'C') {
                        url = '../../../api/CE0016/UpdateDetailsCE'
                    }

                    Ext.Ajax.request({
                        url: url,
                        method: reqVal_p,
                        contentType: "application/json",
                        params: { ITEM_STRING: Ext.util.JSON.encode(data), wh_kinid: T1LastRec.data.CHK_WH_KIND_CODE },
                        success: function (response) {
                            var data = Ext.decode(response.responseText);
                            myMask.hide();
                            if (data.success) {

                                msglabel('訊息區:資料更新成功');
                                T3Load(currentChknos, false);

                            } else {
                                Ext.Msg.alert('失敗', 'Ajax communication failed');
                            }
                        },

                        failure: function (response, action) {
                            myMask.hide();
                            Ext.Msg.alert('失敗', 'Ajax communication failed');
                        }
                    });
                }
            },
            {
                id: 'FinalProIDMulti',
                itemId: 'FinalProIDMulti',
                text: "<span style='color:red'>完成盤點</span>",
                handler: function () {
                    setFinalAll();
                }
            },
            {
                id: 'matchMulti',
                itemId: 'matchMulti', text: '全數符合', handler: function () {

                    var selection = T3Grid.getSelection();

                    if (selection.length == 0) {
                        Ext.Msg.alert('提示', '請選擇項目');
                        return;
                    }

                    var list = [];
                    for (var i = 0; i < selection.length; i++) {
                        var data = selection[i].data;
                        list.push(data);
                    }

                    Ext.Ajax.request({
                        url: '/api/CE0016/Match',
                        method: reqVal_p,
                        params: {
                            list: Ext.util.JSON.encode(list)
                        },
                        success: function (response) {
                            T3Load(currentChknos, false);
                        },
                        failure: function (response, options) {
                            Ext.Msg.alert('失敗', '發生例外錯誤');
                        }
                    });


                }
            },
            {
                id: 'matchMultiCE',
                itemId: 'matchMultiCE', text: "全數符合'", handler: function () {

                    Ext.Ajax.request({
                        url: '/api/CE0016/MatchCE',
                        method: reqVal_p,
                        params: {
                            chk_no: currentChknos,
                            wh_kind: wh_kind
                        },
                        success: function (response) {
                            T3Load(currentChknos, false);
                        },
                        failure: function (response, options) {
                            Ext.Msg.alert('失敗', '發生例外錯誤');
                        }
                    });


                }
            },
        ]
    });
    function getChkTypeColumn(record) {
        
        var type = record.data.CHK_TYPE;
        var pSpan = '<span>';
        if (record.data.STORE_QTYC != record.data.CHK_QTY && record.data.CHK_TIME != "" && record.data.CHK_TIME != null) {
            pSpan = '<span style="color:red">';
        }
        if (record.data.CHK_WH_KIND_CODE == '0') {
            switch (type) {
                case '1':
                    return pSpan + '口</span>';
                case '2':
                    return pSpan + '非口</span>';
                case '3':
                    return pSpan + '1~3管</span>';
                case '4':
                    return pSpan + '4管</span>';
                case '5':
                    return pSpan + '公</span>';
                case '6':
                    return pSpan + '專</span>';
                case '7':
                    return pSpan + '藥</span>';
                case '8':
                    return pSpan + '瓶</span>';
                default:
                    return '<span></span>';
            }
        }

        switch (type) {
            case "0":
                return pSpan + '非</span>';
            case "1":
                return pSpan + '庫</span>';
            case "3":
                return pSpan + '小</span>';
            default:
                return '';
        }
    }
    var T3Grid = Ext.create('Ext.grid.Panel', {
        store: T3Store, //資料load進來
        plain: true,
        loadingText: '處理中...',
        loadMask: true,
        dockedItems: [{
            dock: 'top',
            xtype: 'toolbar',
            layout: 'fit',
            items: [T3Query]
        }, {
            dock: 'top',
            xtype: 'toolbar',
            items: [T3Tool]
        }],
        selModel: {
            checkOnly: false,
            injectCheckbox: 'first',
            mode: 'MULTI'
        },
        selType: 'checkboxmodel',
        enableKeyEvents: true,
        columns: [{
            xtype: 'rownumberer'
        },
        {
            text: "類別",
            dataIndex: 'CHK_TYPE',
            width: 60,
            renderer: function (val, meta, record) {
                return getChkTypeColumn(record);
            },
        },

        {
            text: "院內碼",
            dataIndex: 'MMCODE',
            width: 120,
            renderer: function (value, metaData, record, rowIndex) {
                if (record.data.M_TRNID == '2') {
                    return '<span>' + value + '</span>';
                }
                if (record.data.STORE_QTYC != record.data.CHK_QTY && record.data.CHK_TIME != "" && record.data.CHK_TIME != null) {
                    //metaData.tdStyle = 'background:#FFC8B4;';
                    return '<span style="color:red">' + value + '</span>';
                } else {
                    return '<span>' + value + '</span>';
                }
            }
        }, {
            text: "中文品名",
            dataIndex: 'MMNAME_C',
            width: 200,
            renderer: function (value, metaData, record, rowIndex) {
                if (record.data.M_TRNID == '2') {
                    return '<span>' + value + '</span>';
                }
                if (record.data.STORE_QTYC != record.data.CHK_QTY && record.data.CHK_TIME != "" && record.data.CHK_TIME != null) {
                    //metaData.tdStyle = 'background:#FFC8B4;';
                    return '<span style="color:red">' + value + '</span>';
                } else {
                    return '<span>' + value + '</span>';
                }
            }
        }, {
            text: "英文品名",
            dataIndex: 'MMNAME_E',
            width: 200,
            renderer: function (value, metaData, record, rowIndex) {
                if (record.data.M_TRNID == '2') {
                    return '<span>' + value + '</span>';
                }
                if (record.data.STORE_QTYC != record.data.CHK_QTY && record.data.CHK_TIME != "" && record.data.CHK_TIME != null) {
                    //metaData.tdStyle = 'background:#FFC8B4;';
                    return '<span style="color:red">' + value + '</span>';
                } else {
                    return '<span>' + value + '</span>';
                }
            }
        }, {
            text: "儲位",
            dataIndex: 'STORE_LOC',
            width: 80,
            renderer: function (value, metaData, record, rowIndex) {
                if (record.data.M_TRNID == '2') {
                    return '<span>' + value + '</span>';
                }
                if (record.data.STORE_QTYC != record.data.CHK_QTY && record.data.CHK_TIME != "" && record.data.CHK_TIME != null) {
                    //metaData.tdStyle = 'background:#FFC8B4;';
                    return '<span style="color:red">' + value + '</span>';
                } else {
                    return '<span>' + value + '</span>';
                }
            }
        }, {
            text: "儲位名稱",
            dataIndex: 'LOC_NAME',
            width: 80,
            renderer: function (value, metaData, record, rowIndex) {
                if (record.data.M_TRNID == '2') {
                    return '<span>' + value + '</span>';
                }
                if (record.data.STORE_QTYC != record.data.CHK_QTY && record.data.CHK_TIME != "" && record.data.CHK_TIME != null) {
                    //metaData.tdStyle = 'background:#FFC8B4;';
                    return '<span style="color:red">' + value + '</span>';
                } else {
                    return '<span>' + value + '</span>';
                }
            }
        }, {
            text: "計量單位",
            dataIndex: 'BASE_UNIT',
            width: 80,
            renderer: function (value, metaData, record, rowIndex) {
                if (record.data.M_TRNID == '2') {
                    return '<span>' + value + '</span>';
                }
                if (record.data.STORE_QTYC != record.data.CHK_QTY && record.data.CHK_TIME != "" && record.data.CHK_TIME != null) {
                    //metaData.tdStyle = 'background:#FFC8B4;';
                    return '<span style="color:red">' + value + '</span>';
                } else {
                    return '<span>' + value + '</span>';
                }
            }
        },
        {
            text: "上期結存量",
            dataIndex: 'PRE_INV_QTY',
            align: 'right',
            style: 'text-align:left',
            width: 85,
            renderer: function (value, metaData, record, rowIndex) {
                if (record.data.M_TRNID == '2') {
                    return '<span>' + value + '</span>';
                }
                if (record.data.STORE_QTYC != record.data.CHK_QTY && record.data.CHK_TIME != "" && record.data.CHK_TIME != null) {
                    // metaData.tdStyle = 'background:#FFC8B4;';
                    return '<span style="color:red">' + value + '</span>';
                } else {
                    return '<span>' + value + '</span>';
                }
            }
        }, {
            text: "入庫量",
            dataIndex: 'APL_INQTY',
            align: 'right',
            style: 'text-align:left',
            width: 70,
            renderer: function (value, metaData, record, rowIndex) {
                if (record.data.M_TRNID == '2') {
                    return '<span>' + value + '</span>';
                }
                if (record.data.STORE_QTYC != record.data.CHK_QTY && record.data.CHK_TIME != "" && record.data.CHK_TIME != null) {
                    // metaData.tdStyle = 'background:#FFC8B4;';
                    return '<span style="color:red">' + value + '</span>';
                } else {
                    return '<span>' + value + '</span>';
                }
            }
        },
        {
            text: "出庫量",
            dataIndex: 'APL_OUTQTY',
            align: 'right',
            style: 'text-align:left',
            width: 70,
            renderer: function (value, metaData, record, rowIndex) {
                if (record.data.M_TRNID == '2') {
                    return '<span>' + value + '</span>';
                }
                if (record.data.STORE_QTYC != record.data.CHK_QTY && record.data.CHK_TIME != "" && record.data.CHK_TIME != null) {
                    // metaData.tdStyle = 'background:#FFC8B4;';
                    return '<span style="color:red">' + value + '</span>';
                } else {
                    return '<span>' + value + '</span>';
                }
            }
        }, {
            text: "調帳入",
            dataIndex: 'ADJ_INQTY',
            align: 'right',
            style: 'text-align:left',
            width: 70,
            renderer: function (value, metaData, record, rowIndex) {
                if (record.data.M_TRNID == '2') {
                    return '<span>' + value + '</span>';
                }
                if (record.data.STORE_QTYC != record.data.CHK_QTY && record.data.CHK_TIME != "" && record.data.CHK_TIME != null) {
                    // metaData.tdStyle = 'background:#FFC8B4;';
                    return '<span style="color:red">' + value + '</span>';
                } else {
                    return '<span>' + value + '</span>';
                }
            }
        }, {
            text: "調帳出",
            dataIndex: 'ADJ_OUTQTY',
            align: 'right',
            style: 'text-align:left',
            width: 70,
            renderer: function (value, metaData, record, rowIndex) {
                if (record.data.M_TRNID == '2') {
                    return '<span>' + value + '</span>';
                }
                if (record.data.STORE_QTYC != record.data.CHK_QTY && record.data.CHK_TIME != "" && record.data.CHK_TIME != null) {
                    // metaData.tdStyle = 'background:#FFC8B4;';
                    return '<span style="color:red">' + value + '</span>';
                } else {
                    return '<span>' + value + '</span>';
                }
            }
        }, {
            text: "調撥入",
            dataIndex: 'TRN_INQTY',
            align: 'right',
            style: 'text-align:left',
            width: 70,
            renderer: function (value, metaData, record, rowIndex) {
                if (record.data.M_TRNID == '2') {
                    return '<span>' + value + '</span>';
                }
                if (record.data.STORE_QTYC != record.data.CHK_QTY && record.data.CHK_TIME != "" && record.data.CHK_TIME != null) {
                    // metaData.tdStyle = 'background:#FFC8B4;';
                    return '<span style="color:red">' + value + '</span>';
                } else {
                    return '<span>' + value + '</span>';
                }
            }
        }, {
            text: "調撥出",
            dataIndex: 'TRN_OUTQTY',
            align: 'right',
            style: 'text-align:left',
            width: 70,
            renderer: function (value, metaData, record, rowIndex) {
                if (record.data.M_TRNID == '2') {
                    return '<span>' + value + '</span>';
                }
                if (record.data.STORE_QTYC != record.data.CHK_QTY && record.data.CHK_TIME != "" && record.data.CHK_TIME != null) {
                    // metaData.tdStyle = 'background:#FFC8B4;';
                    return '<span style="color:red">' + value + '</span>';
                } else {
                    return '<span>' + value + '</span>';
                }
            }
        },
        {
            text: "批價扣庫",
            dataIndex: 'USE_QTY',
            align: 'right',
            style: 'text-align:left',
            width: 70,
            renderer: function (value, metaData, record, rowIndex) {
                if (record.data.M_TRNID == '2') {
                    return '<span>' + value + '</span>';
                }
                if (record.data.STORE_QTYC != record.data.CHK_QTY && record.data.CHK_TIME != "" && record.data.CHK_TIME != null) {
                    // metaData.tdStyle = 'background:#FFC8B4;';
                    return '<span style="color:red">' + value + '</span>';
                } else {
                    return '<span>' + value + '</span>';
                }
            }
        },
        {
            text: "電腦量",
            dataIndex: 'STORE_QTYC',
            style: 'text-align:left;',
            align: 'right',
            width: 100,
            renderer: function (value, metaData, record, rowIndex) {
                if (record.data.M_TRNID == '2') {
                    return '<span>' + value + '</span>';
                }
                if (record.data.STORE_QTYC != record.data.CHK_QTY && record.data.CHK_TIME != "" && record.data.CHK_TIME != null) {
                    //metaData.tdStyle = 'background:#FFC8B4;';
                    return '<span style="color:red">' + value + '</span>';
                } else {
                    return '<span>' + value + '</span>';
                }
            }
        }, {
            text: "盤點量",
            style: 'text-align:left; color:red',
            align: 'right',
            dataIndex: 'CHK_QTY',
            width: 70,
            editor: {
                xtype: 'textfield',
                regexText: '只能輸入數字',
                regex: /^[0-9]+$/, // 用正規表示式限制可輸入內容
                selectOnFocus: true,
                listeners: {
                    change: function (field, newVal, oldVal) {
                    },
                }
            },
            renderer: function (value, metaData, record, rowIndex) {
                if (record.data.CHK_QTY == null) {
                    return '';
                }
                if (record.data.M_TRNID == '2') {
                    return '<span>' + value + '</span>';
                }
                if (record.data.STORE_QTYC != record.data.CHK_QTY && record.data.CHK_TIME != "" && record.data.CHK_TIME != null) {
                    //metaData.tdStyle = 'background:#FFC8B4;';
                    return '<span style="color:red">' + value + '</span>';
                } else {
                    return '<span>' + value + '</span>';
                }
            }
        },
        {
            text: "盤點人員",
            dataIndex: 'CHK_UID_NAME',
            width: 90,
            renderer: function (value, metaData, record, rowIndex) {
                if (record.data.M_TRNID == '2') {
                    return '<span>' + value + '</span>';
                }
                if (record.data.STORE_QTYC != record.data.CHK_QTY && record.data.CHK_TIME != "" && record.data.CHK_TIME != null) {
                    //metaData.tdStyle = 'background:#FFC8B4;';
                    return '<span style="color:red">' + value + '</span>';
                } else {
                    return '<span>' + value + '</span>';
                }
            }
        },
        {
            text: "盤點時間",
            dataIndex: 'CHK_TIME',
            width: 120,
            renderer: function (value, meta, record) {
                if (value == "Mon Jan 01 0001 00:00:00 GMT+0806 (Taipei Standard Time)") {  //value值是 null時候
                    return '';
                } else if (value == "Mon Jan 01 0001 00:00:00 GMT+0806 (台北標準時間)") {
                    return '';
                } else {
                    if (record.data.M_TRNID == '2') {
                        return '<span>' + Ext.util.Format.date(value, 'Xmd H:i:s') + '</span>';
                    }
                    if (record.data.STORE_QTYC != record.data.CHK_QTY && record.data.CHK_TIME != "" && record.data.CHK_TIME != null) {
                        return '<span style="color:red">' + Ext.util.Format.date(value, 'Xmd H:i:s') + '</span>';
                    } else {
                        return '<span>' + Ext.util.Format.date(value, 'Xmd H:i:s') + '</span>';
                    }
                }
            }
        },
        //{
        //    text: "扣庫方式",
        //    dataIndex: 'M_TRNID',
        //    width: 80,
        //    renderer: function (value, metaData, record, rowIndex) {
        //        var display = '扣庫';
        //        if (value == '2') {
        //            display = '不扣庫';
        //        }

        //        if (record.data.M_TRNID == '2') {
        //            return '<span>' + display + '</span>';
        //        }
        //        if (record.data.STORE_QTYC != record.data.CHK_QTY && record.data.CHK_TIME != "" && record.data.CHK_TIME != null) {
        //            //metaData.tdStyle = 'background:#ffe1e1;';
        //            return '<span style="color:red">' + display + '</span>';
        //        } else {
        //            return '<span>' + display + '</span>';
        //        }
        //    }
        //},
        {
            dataIndex: 'STATUS_INI',
            width: '10%',
            hidden: true
        },
        {
            header: "",
            flex: 1
        }],
        plugins: [
            Ext.create('Ext.grid.plugin.CellEditing', {
                clicksToEdit: 1
            })
        ],
        listeners: {
            beforeedit: function (editor, e) {
                if (isEditable(multiCheckList[0].CHK_YM) == false) {
                    return false;
                }
                // STATUS_INI不是1 則不可填寫
                if (e.colIdx === 10 && e.record.get('STATUS_INI') != '1') {
                    return false;
                }
                
                //disable 儲存,完成盤點 (須參考)
                if (e.record.get('STATUS_INI') != '1') {
                    T3Grid.down('#updateMulti').setDisabled(true);
                    T3Grid.down('#FinalProIDMulti').setDisabled(true);
                    T3Grid.down('#matchMulti').setDisabled(true);

                }
            }
        },
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
                    //    T2Load();
                    //}

                    //alert('The press key is' + e.getKey());
                }
            }
        },
    });
    var multiCheckWindow = Ext.create('Ext.window.Window', {
        renderTo: Ext.getBody(),
        id: 'multiCheckWindow',
        width: windowWidth,
        height: windowHeight,
        xtype: 'form',
        layout: 'form',
        closable: false,
        layout: {
            type: 'fit',
            padding: 0
        },
        defaults: {
            split: true  //可以調整大小
        },
        items: [{
            itemId: 't3Grid',
            region: 'center',
            layout: 'fit',
            split: true,
            collapsible: false,
            border: false,
            items: [T3Grid]
        }],
        buttons: [
            {
                id: 'backBtnMulti',
                disabled: false,
                text: '關閉',
                handler: function () {
                    var tempData = T3Grid.getStore().data.items;
                    var if_dirty = "no";
                    for (var i = 0; i < tempData.length; i++) {
                        if (tempData[i].dirty) {
                            if_dirty = "yes";
                            break;
                        }
                    }
                    if (if_dirty == "no") {
                        this.up('window').hide();
                    } else {
                        Ext.MessageBox.confirm(
                            "提醒",
                            "您的資料未儲存!確定要回上一頁?",
                            function (btn) {
                                if (btn == "yes") {
                                    T3Tool.suspendEvents();
                                    var win = Ext.getCmp('multiCheckWindow');
                                    win.hide();
                                }
                            });
                    }
                }
            }]
    });
    //#endregion

    //#region 多筆列印
    var T25Form = Ext.widget({
        xtype: 'form',
        layout: 'form',
        border: false,
        autoScroll: true, // 若為true,容器內容超出容器大小時出現捲動條;若為false超出容器的部分會被裁切掉
        fieldDefaults: {
            labelAlign: 'right',
            msgTarget: 'side',
            labelWidth: 70
        },
        items: [{
            xtype: 'container',
            layout: 'vbox',
            items: [
                {
                    xtype: 'panel',
                    border: false,
                    layout: 'hbox',
                    width: 250,
                    items: [
                        {
                            xtype: 'radiogroup',
                            name: 'multiPrintType',
                            fieldLabel: '列印格式',
                            items: [
                                { boxLabel: '依院內碼', width: 70, name: 'multiPrintType', inputValue: 'mmcode', checked: true },
                                { boxLabel: '依單號', width: 70, name: 'multiPrintType', inputValue: 'chk_no' },
                            ]
                        },
                    ]
                },
            ]
        }
        ]
    });

    var multiPrintWindow = Ext.create('Ext.window.Window', {
        renderTo: Ext.getBody(),
        modal: true,
        items: [
            T25Form
        ],
        resizable: false,
        draggable: false,
        closable: false,
        title: "多筆列印",
        buttons: [
            {
                text: '確定',
                handler: function () {

                    var selection = T1Grid.getSelection();

                    var chk_nos = "";
                    for (var i = 0; i < selection.length; i++) {
                        if (i == 0) {
                            chk_nos = selection[i].data.CHK_NO;
                        } else {
                            chk_nos = chk_nos + "," + selection[i].data.CHK_NO;
                        }
                    }

                    if (T25Form.getForm().findField('multiPrintType').getValue()['multiPrintType'] == 'mmcode') {
                        if (selection[0].data.CHK_WH_KIND == 'E' || selection[0].data.CHK_WH_KIND == 'C') {
                            showReport(reportMultiMmcodeUrl, chk_nos);
                        } else {
                            showReport(reportMultiMmcodeWardUrl, chk_nos);
                        }
                    } else {
                        showReport(reportMultiChknoUrl, chk_nos);
                    }
                }
            },
            {
                text: '取消',
                handler: function () {
                    multiPrintWindow.hide();
                }
            }
        ]
    });
    multiPrintWindow.hide();
    //#endregion 

    Ext.on('resize', function () {
        windowHeight = $(window).height();
        windowWidth = $(window).width();
        popformINI.setHeight(windowHeight);
        popformINI.setWidth(windowWidth);
        multiCheckWindow.setHeight(windowHeight);
        multiCheckWindow.setWidth(windowWidth);
    });

    //T1Load(); // 進入畫面時自動載入一次資料
    //T1Query.getForm().findField('P0').focus();
});